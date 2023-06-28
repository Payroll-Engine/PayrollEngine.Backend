﻿//#define VALUE_CALC_PERFORMANCE
//#define CASE_VALUE_RESULT_PERFORMANCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting;
using PayrollEngine.Domain.Scripting.Controller;

namespace PayrollEngine.Domain.Application;

internal sealed class PayrunProcessorRegulation
{
    private IFunctionHost FunctionHost { get; }
    private PayrunProcessorSettings Settings { get; }
    private IResultProvider ResultProvider { get; }
    private Tenant Tenant { get; }
    private Payroll Payroll { get; }
    private Payrun Payrun { get; }

    internal PayrunProcessorRegulation(IFunctionHost functionHost, PayrunProcessorSettings settings,
        IResultProvider resultProvider, Tenant tenant, Payroll payroll, Payrun payrun)
    {
        FunctionHost = functionHost ?? throw new ArgumentNullException(nameof(functionHost));
        ResultProvider = resultProvider ?? throw new ArgumentNullException(nameof(resultProvider));
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
        Payroll = payroll ?? throw new ArgumentNullException(nameof(payroll));
        Payrun = payrun ?? throw new ArgumentNullException(nameof(payrun));
    }

    #region Regulation

    internal async Task<ILookup<string, Collector>> GetDerivedCollectorsAsync(PayrunJob payrunJob, ClusterSet clusterSet)
    {
        var collectors = (await Settings.PayrollRepository.GetDerivedCollectorsAsync(Settings.DbContext,
            new()
            {
                TenantId = Tenant.Id,
                PayrollId = Payroll.Id,
                RegulationDate = payrunJob.PeriodEnd,
                EvaluationDate = payrunJob.EvaluationDate
            },
            clusterSet: clusterSet,
            overrideType: OverrideType.Active)).ToList();
        return collectors.ToLookup(col => col.Name, col => col);
    }

    internal async Task<ILookup<decimal, WageType>> GetDerivedWageTypesAsync(PayrunJob payrunJob, ClusterSet clusterSet)
    {
        var deriveWageTypes = (await Settings.PayrollRepository.GetDerivedWageTypesAsync(Settings.DbContext,
            new()
            {
                TenantId = Tenant.Id,
                PayrollId = Payroll.Id,
                RegulationDate = payrunJob.PeriodEnd,
                EvaluationDate = payrunJob.EvaluationDate
            },
            clusterSet: clusterSet,
            overrideType: OverrideType.Active)).ToList();
        return deriveWageTypes.ToLookup(wt => wt.WageTypeNumber, wt => wt);
    }

    internal async Task<ILookup<string, Lookup>> GetDerivedLookupsAsync(PayrunJob payrunJob)
    {
        var lookups = (await Settings.PayrollRepository.GetDerivedLookupsAsync(Settings.DbContext,
            new()
            {
                TenantId = Tenant.Id,
                PayrollId = Payroll.Id,
                RegulationDate = payrunJob.PeriodEnd,
                EvaluationDate = payrunJob.EvaluationDate
            },
            overrideType: OverrideType.Active)).ToList();
        return lookups.ToLookup(lookup => lookup.Name, col => col);
    }

    #endregion

    #region Wage Type

    internal bool IsWageTypeAvailable(PayrunContext context, IGrouping<decimal, WageType> derivedWageType,
        ICaseValueProvider caseValueProvider)
    {
        Log.Trace($"checking availability of wage type {derivedWageType.Key}");

        if (string.IsNullOrWhiteSpace(Payrun.WageTypeAvailableExpression))
        {
            return true;
        }

        // current wage with attributes
        var wageType = derivedWageType.First();
        var wageTypeAttributes = derivedWageType.ToList().CollectDerivedAttributes(wt => wt.Attributes);

        // execute wage type available script
        var isAvailable = new PayrunScriptController().IsWageTypeAvailable(wageType, wageTypeAttributes,
            new()
            {
                DbContext = Settings.DbContext,
                Culture = context.Culture,
                FunctionHost = FunctionHost,
                Tenant = Tenant,
                User = context.User,
                Payroll = context.Payroll,
                Payrun = Payrun,
                PayrunJob = context.PayrunJob,
                ParentPayrunJob = context.ParentPayrunJob,
                ExecutionPhase = context.ExecutionPhase,
                ResultProvider = ResultProvider,
                CaseValueProvider = caseValueProvider,
                RegulationLookupProvider = context.RegulationLookupProvider,
                RuntimeValueProvider = context.RuntimeValueProvider,
                WebhookDispatchService = Settings.WebhookDispatchService
            });
        return isAvailable ?? true;
    }

    internal Tuple<WageTypeResultSet, List<RetroPayrunJob>, List<string>, bool> CalculateWageTypeValue(PayrunContext context, IGrouping<decimal, WageType> derivedWageType,
        PayrollResultSet currentPayrollResult, ICaseValueProvider caseValueProvider, int executionCount)
    {
        var retroPayrunJobs = new List<RetroPayrunJob>();

#if VALUE_CALC_PERFORMANCE
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Restart();
#endif

        // wage type
        Log.Trace($"Calculating value of wage type {derivedWageType.Key} on the object {derivedWageType.Key}");

        // default wage type result
        var wageType = derivedWageType.First();
        var resultSet = new WageTypeResultSet
        {
            WageTypeId = wageType.Id,
            WageTypeNumber = wageType.WageTypeNumber,
            WageTypeName = wageType.Name,
            WageTypeNameLocalizations = wageType.NameLocalizations,
            ValueType = wageType.ValueType,
            Start = context.PayrunJob.PeriodStart,
            End = context.PayrunJob.PeriodEnd,
            Attributes = new(),
            CustomResults = new()
        };

        // custom period results by cluster
        var clusterSetPeriodResult = context.Payroll.ClusterSets?.FirstOrDefault(x => string.Equals(context.Payroll.ClusterSetWageTypePeriod, x.Name));
        var autoPeriodResults = clusterSetPeriodResult != null && derivedWageType.AvailableCluster(clusterSetPeriodResult);

        // disabled collectors
        var disabledCollectors = new List<string>();

        //  wage type value script
        decimal? wageTypeValue = null;
        var valueExpressions = derivedWageType.GetDerivedExpressionObjects(x => x.ValueExpression).ToList();
        while (valueExpressions.Count > 0)
        {
            // current wage type with attributes
            var evalWageType = valueExpressions.First();
            var wageTypeAttributes = valueExpressions.CollectDerivedAttributes(wt => wt.Attributes);

            // execute wage type value script
            var result = new WageTypeScriptController().GetValue(context.CaseFieldProvider, new()
            {
                DbContext = Settings.DbContext,
                Culture = context.Culture,
                FunctionHost = FunctionHost,
                Tenant = Tenant,
                User = context.User,
                Payroll = context.Payroll,
                ExecutionCount = executionCount,
                WageType = evalWageType,
                WageTypeAttributes = wageTypeAttributes,
                DisabledCollectors = disabledCollectors,
                Payrun = Payrun,
                PayrunJob = context.PayrunJob,
                ParentPayrunJob = context.ParentPayrunJob,
                ExecutionPhase = context.ExecutionPhase,
                ResultProvider = ResultProvider,
                RegulationLookupProvider = context.RegulationLookupProvider,
                CaseValueProvider = caseValueProvider,
                CurrentPayrollResult = currentPayrollResult,
                CurrentWageTypeResult = resultSet,
                RuntimeValueProvider = context.RuntimeValueProvider,
                WebhookDispatchService = Settings.WebhookDispatchService
            }, autoPeriodResults);

            // execution restart
            if (result != null && result.Item3)
            {
                return new(new(), new(), new(), true);
            }

            // wage type value
            wageTypeValue = result?.Item1;

            // retro payrun jobs
            if (result != null)
            {
                AddRetroPayrunJobs(retroPayrunJobs, result.Item2, context.EvaluationPeriod.Start);
            }

            if (wageTypeValue != null)
            {
                break;
            }

            // process derived base, required to reduce the collected attributes
            valueExpressions.RemoveAt(0);
        }

        // empty result
        if (wageTypeValue == null)
        {
            return null;
        }

        // apply wage type value
        resultSet.Value = wageTypeValue.Value;

        // wage type result script: special case with reverse/bottom-up order
        var resultExpressions = derivedWageType.GetDerivedExpressionObjects(x => x.ResultExpression).Reverse().ToList();
        while (resultExpressions.Count > 0)
        {
            // current wage type
            var evalWageType = resultExpressions.First();
            var wageTypeAttributes = evalWageType.Attributes ?? new Dictionary<string, object>();
            // execute wage type result script
            var retroJobs = new WageTypeScriptController().Result(resultSet.Value, new()
            {
                DbContext = Settings.DbContext,
                Culture = context.Culture,
                FunctionHost = FunctionHost,
                Tenant = Tenant,
                User = context.User,
                Payroll = context.Payroll,
                ExecutionCount = executionCount,
                WageType = evalWageType,
                WageTypeAttributes = wageTypeAttributes,
                DisabledCollectors = disabledCollectors,
                Payrun = Payrun,
                PayrunJob = context.PayrunJob,
                ExecutionPhase = context.ExecutionPhase,
                ParentPayrunJob = context.ParentPayrunJob,
                ResultProvider = ResultProvider,
                RegulationLookupProvider = context.RegulationLookupProvider,
                CaseValueProvider = caseValueProvider,
                CurrentPayrollResult = currentPayrollResult,
                CurrentWageTypeResult = resultSet,
                RuntimeValueProvider = context.RuntimeValueProvider,
                WebhookDispatchService = Settings.WebhookDispatchService
            });

            // retro payrun jobs
            AddRetroPayrunJobs(retroPayrunJobs, retroJobs, context.EvaluationPeriod.Start);

            // process next derived
            resultExpressions.RemoveAt(0);
        }

#if VALUE_CALC_PERFORMANCE
            stopwatch.Stop();
            Log.Information($"Calculate wage type {wageType.WageTypeNumber}: {stopwatch.ElapsedMilliseconds} ms");
#endif

        return new(resultSet, retroPayrunJobs, disabledCollectors, false);
    }
    #endregion

    #region Collector

    internal static bool IsCollectorAvailable(IGrouping<decimal, WageType> derivedWageType, IGrouping<string, Collector> derivedCollector)
    {
        Log.Trace($"checking availability of collector {derivedCollector.Key} on wage type {derivedWageType.Key}");

        var collector = derivedCollector.First();
        var wageType = derivedWageType.First();
        return wageType.CollectorAvailable(collector.Name, collector.CollectorGroups);
    }

    internal List<RetroPayrunJob> CollectorStart(PayrunContext context, IGrouping<string, Collector> derivedCollector,
        ICaseValueProvider caseValueProvider, PayrollResultSet currentPayrollResult, CollectorResultSet collectorResult)
    {
        var retroPayrunJobs = new List<RetroPayrunJob>();

        // start expression
        var startExpressions = derivedCollector.GetDerivedExpressionObjects(x => x.StartExpression).ToList();
        while (startExpressions.Count > 0)
        {
            // current collector with attributes
            var startCollector = startExpressions.First();
            startCollector.Attributes = startExpressions.CollectDerivedAttributes(col => col.Attributes);

            // execute collector start script
            var retroJobs = new CollectorScriptController().Start(new()
            {
                DbContext = Settings.DbContext,
                Culture = context.Culture,
                FunctionHost = FunctionHost,
                Tenant = Tenant,
                User = context.User,
                Payroll = context.Payroll,
                Collector = startCollector,
                Payrun = Payrun,
                PayrunJob = context.PayrunJob,
                ParentPayrunJob = context.ParentPayrunJob,
                ExecutionPhase = context.ExecutionPhase,
                ResultProvider = ResultProvider,
                RegulationLookupProvider = context.RegulationLookupProvider,
                CaseValueProvider = caseValueProvider,
                CurrentPayrollResult = currentPayrollResult,
                CurrentCollectorResult = collectorResult,
                RuntimeValueProvider = context.RuntimeValueProvider,
                WebhookDispatchService = Settings.WebhookDispatchService
            });

            // retro payrun jobs
            AddRetroPayrunJobs(retroPayrunJobs, retroJobs, context.EvaluationPeriod.Start);

            // process derived base, required to reduce the collected attributes
            startExpressions.RemoveAt(0);
        }

        return retroPayrunJobs;
    }

    internal Tuple<decimal, List<RetroPayrunJob>> CollectorApply(PayrunContext context, IGrouping<string, Collector> derivedCollector,
        ICaseValueProvider caseValueProvider, WageTypeResult wageTypeResult,
        PayrollResultSet currentPayrollResult, CollectorResultSet collectorResult)
    {
        // value
        decimal? value = null;
        var retroPayrunJobs = new List<RetroPayrunJob>();
        var collector = derivedCollector.First();

        // apply expression
        var applyExpressions = derivedCollector.GetDerivedExpressionObjects(x => x.ApplyExpression).ToList();
        while (applyExpressions.Count > 0)
        {
            // current collector with attributes
            var applyCollector = applyExpressions.First();
            applyCollector.Attributes = applyExpressions.CollectDerivedAttributes(col => col.Attributes);

            // execute collector apply script
            var result = new CollectorScriptController().ApplyValue(wageTypeResult, new()
            {
                DbContext = Settings.DbContext,
                Culture = context.Culture,
                FunctionHost = FunctionHost,
                Tenant = Tenant,
                User = context.User,
                Payroll = context.Payroll,
                Collector = applyCollector,
                Payrun = Payrun,
                PayrunJob = context.PayrunJob,
                ParentPayrunJob = context.ParentPayrunJob,
                ExecutionPhase = context.ExecutionPhase,
                ResultProvider = ResultProvider,
                RegulationLookupProvider = context.RegulationLookupProvider,
                CaseValueProvider = caseValueProvider,
                CurrentPayrollResult = currentPayrollResult,
                CurrentCollectorResult = collectorResult,
                RuntimeValueProvider = context.RuntimeValueProvider,
                WebhookDispatchService = Settings.WebhookDispatchService
            });
            value = result.Item1;
            if (value != null)
            {
                // retro payrun jobs
                AddRetroPayrunJobs(retroPayrunJobs, result.Item2, context.EvaluationPeriod.Start);

                // value provided by the function
                break;
            }

            // process derived base, required to reduce the collected attributes
            applyExpressions.RemoveAt(0);
        }

        // in case of missing collector apply function: fallback to wage type value
        value ??= wageTypeResult.Value;

        // update the most derived collector
        collector.AddValue(value.Value);

        return new(collector.Result, retroPayrunJobs);
    }

    internal List<RetroPayrunJob> CollectorEnd(PayrunContext context, IGrouping<string, Collector> derivedCollector,
        ICaseValueProvider caseValueProvider, PayrollResultSet currentPayrollResult,
        CollectorResultSet collectorResult)
    {
        var retroPayrunJobs = new List<RetroPayrunJob>();

        // end expression
        var endExpressions = derivedCollector.GetDerivedExpressionObjects(x => x.EndExpression).ToList();
        while (endExpressions.Count > 0)
        {
            // current collector with attributes
            var endCollector = endExpressions.First();
            endCollector.Attributes = endExpressions.CollectDerivedAttributes(col => col.Attributes);

            // execute collector end script
            var retroJobs = new CollectorScriptController().End(new()
            {
                DbContext = Settings.DbContext,
                Culture = context.Culture,
                FunctionHost = FunctionHost,
                Tenant = Tenant,
                User = context.User,
                Payroll = context.Payroll,
                Collector = endCollector,
                Payrun = Payrun,
                PayrunJob = context.PayrunJob,
                ParentPayrunJob = context.ParentPayrunJob,
                ExecutionPhase = context.ExecutionPhase,
                ResultProvider = ResultProvider,
                RegulationLookupProvider = context.RegulationLookupProvider,
                CaseValueProvider = caseValueProvider,
                CurrentPayrollResult = currentPayrollResult,
                CurrentCollectorResult = collectorResult,
                RuntimeValueProvider = context.RuntimeValueProvider,
                WebhookDispatchService = Settings.WebhookDispatchService
            });

            // retro payrun jobs
            AddRetroPayrunJobs(retroPayrunJobs, retroJobs, context.EvaluationPeriod.Start);

            // process derived base, required to reduce the collected attributes
            endExpressions.RemoveAt(0);
        }

        return retroPayrunJobs;
    }

    internal async Task<List<PayrunResult>> GetCaseValuePayrunResultsAsync(Payroll payroll,
        PayrunJob payrunJob, ICaseValueProvider caseValueProvider, bool expandCaseSlots)
    {
        var payrunResults = new List<PayrunResult>();

        // clusters
        if (string.IsNullOrWhiteSpace(payroll.ClusterSetCaseValue))
        {
            return payrunResults;
        }

        ClusterSet clusterSet = null;
        if (!string.Equals(payroll.ClusterSetCaseValue, ClusterSet.SetNameAll))
        {
            clusterSet = payroll.GetClusterSet(payroll.ClusterSetCaseValue);
            if (clusterSet == null)
            {
                return payrunResults;
            }
        }

#if CASE_VALUE_RESULT_PERFORMANCE
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
#endif

        // cases, only available for slot payrun results
        List<Case> cases = null;
        if (expandCaseSlots)
        {
            cases = (await Settings.PayrollRepository.GetDerivedCasesAsync(Settings.DbContext,
                new()
                {
                    TenantId = Tenant.Id,
                    PayrollId = payroll.Id,
                    RegulationDate = payrunJob.PeriodEnd,
                    EvaluationDate = payrunJob.EvaluationDate
                })).ToList();
        }

#if CASE_VALUE_RESULT_PERFORMANCE
            stopwatch.Stop();
            Log.Information($"Get cases: {stopwatch.ElapsedMilliseconds} ms");
            stopwatch.Restart();
#endif

        // case fields
        var caseFields = await Settings.PayrollRepository.GetDerivedCaseFieldsAsync(Settings.DbContext,
            new()
            {
                TenantId = Tenant.Id,
                PayrollId = payroll.Id,
                RegulationDate = payrunJob.PeriodEnd,
                EvaluationDate = payrunJob.EvaluationDate
            },
            clusterSet: clusterSet);

#if CASE_VALUE_RESULT_PERFORMANCE
            stopwatch.Stop();
            Log.Information($"Get case fields: {stopwatch.ElapsedMilliseconds} ms");
#endif

        foreach (var caseField in caseFields)
        {
            // case slots
            var slots = new List<CaseSlot>();
            if (cases != null)
            {
                var @case = cases.FirstOrDefault(x => x.Id == caseField.CaseId);
                if (@case != null && @case.Slots != null)
                {
                    slots.AddRange(@case.Slots);
                }
            }
            // case slot values
            if (slots.Any())
            {
                foreach (var slot in slots)
                {
                    payrunResults.AddRange(await GetCaseValuePayrunResultsAsync(caseValueProvider, caseField, payrunJob.PeriodEnd, slot.Name));
                }
            }
            else
            {
                // case value
                payrunResults.AddRange(await GetCaseValuePayrunResultsAsync(caseValueProvider, caseField, payrunJob.PeriodEnd));
            }
        }
        return payrunResults;
    }

    private static async Task<List<PayrunResult>> GetCaseValuePayrunResultsAsync(ICaseValueProvider caseValueProvider, ChildCaseField caseField,
        DateTime periodEnd, string caseSlot = null)
    {
        var results = new List<PayrunResult>();

        var caseValuePeriods = await caseValueProvider.GetCaseValueSplitPeriodsAsync(caseField.Name, caseField.CaseType, caseSlot);
        if (caseValuePeriods.Any())
        {
            foreach (var caseValuePeriod in caseValuePeriods)
            {
                // case value as payrun result
                var caseValue = caseValuePeriod.Key;
                foreach (var datePeriod in caseValuePeriod.Value)
                {
                    results.Add(new()
                    {
                        Source = caseField.CaseType.ToString(),
                        Name = caseField.Name,
                        NameLocalizations = caseField.NameLocalizations,
                        Slot = caseSlot,
                        ValueType = caseField.ValueType,
                        Start = datePeriod.Start,
                        End = datePeriod.End == periodEnd ? datePeriod.End : datePeriod.End.PreviousTick(),
                        Value = caseValue.Value,
                        NumericValue = caseValue.NumericValue,
                        Tags = caseField.Tags,
                        Attributes = caseValue.Attributes
                    });
                }
            }
        }

        return results;
    }

    #endregion

    private static void AddRetroPayrunJobs(List<RetroPayrunJob> payrunJobs, List<RetroPayrunJob> retroJobs, DateTime evaluationPeriodStart)
    {
        if (retroJobs != null && retroJobs.Any())
        {
            foreach (var payrunJob in retroJobs)
            {
                if (payrunJob.ScheduleDate > evaluationPeriodStart)
                {
                    throw new PayrollException(
                        $"Retro job schedule date {payrunJob.ScheduleDate} must be before {evaluationPeriodStart}");
                }
                payrunJobs.Add(payrunJob);
            }
        }
    }

}
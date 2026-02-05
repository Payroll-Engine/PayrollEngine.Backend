using System;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting.Controller;
using Calendar = PayrollEngine.Domain.Model.Calendar;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Process a payrun job
/// </summary>
public class PayrunProcessor : FunctionToolBase
{
    private sealed class PayrunSetup
    {
        internal Payroll Payroll { get; init; }
        internal Division Division { get; init; }
        internal List<Employee> Employees { get; init; }
        internal CaseValueCache GlobalCaseValues { get; init; }
        internal CaseValueCache NationalCaseValues { get; init; }
        internal CaseValueCache CompanyCaseValues { get; init; }
        internal CaseValueCache EmployeeCaseValues { get; init; }
    }

    // global
    private Tenant Tenant { get; }
    private Payrun Payrun { get; }

    // internal
    private new PayrunProcessorSettings Settings => base.Settings as PayrunProcessorSettings;
    private IResultProvider ResultProvider { get; }
    private bool LogWatch { get; }

    public PayrunProcessor(Tenant tenant, Payrun payrun, PayrunProcessorSettings settings) :
        base(settings)
    {
        // global
        Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
        Payrun = payrun ?? throw new ArgumentNullException(nameof(payrun));

        // internal
        ResultProvider = new ResultProvider(Settings.PayrollResultRepository, Settings.PayrollConsolidatedResultRepository);
        LogWatch = Settings.FunctionLogTimeout != TimeSpan.Zero;
    }

    #region Process

    public async Task<PayrunJob> Process(PayrunJobInvocation jobInvocation) =>
        await Process(jobInvocation, new());

    private async Task<PayrunJob> Process(PayrunJobInvocation jobInvocation, PayrunSetup setup)
    {
        // setup
        if (setup.EmployeeCaseValues != null && (setup.Employees == null || setup.Employees.Count != 1))
        {
            throw new ArgumentException("Invalid payrun employee setup", nameof(setup));
        }

        // log level
        FunctionHost.LogLevel = jobInvocation.LogLevel;

        // processor repositories
        var processorRepositories = new PayrunProcessorRepositories(Settings, Tenant);

        // resolve payroll from payrun
        var payrun = await processorRepositories.LoadPayrunAsync(jobInvocation.PayrunId);
        var payrollId = payrun.PayrollId;

        // payrun context
        var context = new PayrunContext
        {
            // user
            User = await processorRepositories.LoadUserAsync(jobInvocation.UserId),
            // context payroll
            Payroll = setup.Payroll ?? await processorRepositories.LoadPayrollAsync(payrollId),
            // retro pay
            RetroDate = await GetRetroDateAsync(jobInvocation),
            // empty results
            StoreEmptyResults = jobInvocation.StoreEmptyResults
        };

        // context division
        context.Division = setup.Division ?? await processorRepositories.LoadDivisionAsync(context.Payroll.DivisionId);

        // [culture by priority]: division > tenant > system</remarks>
        var cultureName =
            // priority 1: division culture
            context.Division.Culture ??
            // priority 2: tenant culture
            Tenant.Culture ??
            // priority 3: system culture
            CultureInfo.CurrentCulture.Name;
        context.PushPayrollCulture(cultureName);

        // calendar
        var calendarName = context.Division.Calendar ?? Tenant.Calendar;
        context.CalendarName = calendarName;

        // calculator
        context.Calculator = await GetCalculatorAsync(
            tenantId: Tenant.Id,
            userId: context.User.Id,
            culture: context.PayrollCulture,
            calendarName: context.CalendarName);

        // create payrun job and retro payrun jobs
        // Check if job was pre-created by async controller
        if (jobInvocation.PayrunJobId > 0)
        {
            // Job already created by async controller - load and update it
            context.PayrunJob = await processorRepositories.LoadPayrunJobAsync(jobInvocation.PayrunJobId);
            if (context.PayrunJob == null)
            {
                throw new PayrunException($"Payrun job with id {jobInvocation.PayrunJobId} not found");
            }
            // Update job with calculated period/cycle info from calculator
            PayrunJobFactory.UpdatePayrunJob(
                payrunJob: context.PayrunJob,
                jobInvocation: jobInvocation,
                divisionId: context.Division.Id,
                payrollId: payrollId,
                payrollCalculator: context.Calculator);
        }
        else
        {
            // Create new job (sync mode or retro jobs)
            context.PayrunJob = PayrunJobFactory.CreatePayrunJob(
                jobInvocation: jobInvocation,
                divisionId: context.Division.Id,
                payrollId: payrollId,
                payrollCalculator: context.Calculator);
        }
        if (context.PayrunJob.ParentJobId.HasValue)
        {
            context.ParentPayrunJob = await processorRepositories.LoadPayrunJobAsync(context.PayrunJob.ParentJobId.Value);
            context.RetroPayrunJobs = jobInvocation.RetroJobs;
        }
        // update invocation (only needed for new jobs)
        if (jobInvocation.PayrunJobId == 0)
        {
            jobInvocation.PayrunJobId = context.PayrunJob.Id;
        }

        // context dates
        context.EvaluationDate = context.PayrunJob.EvaluationDate;
        context.EvaluationPeriod = context.PayrunJob.GetEvaluationPeriod();

        // derived regulation
        context.DerivedRegulations = await processorRepositories.LoadDerivedRegulationsAsync(
            payrollId: context.Payroll.Id,
            regulationDate: context.PayrunJob.PeriodEnd,
            evaluationDate: context.EvaluationDate);

        // context case field provider
        ClusterSet caseFieldClusterSet = null;
        if (!string.IsNullOrWhiteSpace(context.Payroll.ClusterSetCaseField))
        {
            caseFieldClusterSet = context.Payroll.ClusterSets.FirstOrDefault(x => string.Equals(context.Payroll.ClusterSetCaseField, x.Name));
        }
        context.CaseFieldProvider = new CaseFieldProvider(
            new CaseFieldProxyRepository(Settings.PayrollRepository, Tenant.Id, Payrun.PayrollId,
                context.PayrunJob.PeriodEnd, context.EvaluationDate, caseFieldClusterSet));

        // context global, national and company case values
        context.GlobalCaseValues = setup.GlobalCaseValues ??
                                   new CaseValueCache(
                                       context: Settings.DbContext,
                                       caseValueRepository: Settings.GlobalCaseValueRepository,
                                       parentId: Tenant.Id,
                                       divisionId: context.Division.Id,
                                       evaluationDate: context.EvaluationDate,
                                       forecast: context.PayrunJob.Forecast);
        context.NationalCaseValues = setup.NationalCaseValues ??
                                     new CaseValueCache(
                                         context: Settings.DbContext,
                                         caseValueRepository: Settings.NationalCaseValueRepository,
                                         parentId: Tenant.Id,
                                         divisionId: context.Division.Id,
                                         evaluationDate: context.EvaluationDate,
                                         forecast: context.PayrunJob.Forecast);
        context.CompanyCaseValues = setup.CompanyCaseValues ??
                                    new CaseValueCache(
                                        context: Settings.DbContext,
                                        caseValueRepository: Settings.CompanyCaseValueRepository,
                                        parentId: Tenant.Id,
                                        divisionId: context.Division.Id,
                                        evaluationDate: context.EvaluationDate,
                                        forecast: context.PayrunJob.Forecast);

        // payrun processor regulation
        var processorRegulation = new PayrunProcessorRegulation(
            functionHost: FunctionHost,
            settings: Settings,
            resultProvider: ResultProvider,
            tenant: Tenant,
            payroll: context.Payroll,
            payrun: Payrun);

        // context derived lookups by lookup name
        context.RegulationLookupProvider = new RegulationLookupProvider(
            dbContext: Settings.DbContext,
            payrollRepository: Settings.PayrollRepository,
            payrollQuery: new()
            {
                TenantId = Tenant.Id,
                PayrollId = context.Payroll.Id,
                RegulationDate = context.PayrunJob.PeriodEnd,
                EvaluationDate = context.PayrunJob.EvaluationDate
            },
            regulationRepository: Settings.RegulationRepository,
            lookupSetRepository: Settings.RegulationLookupSetRepository);

        // derived collectors (optional)
        context.DerivedCollectors = await GetDerivedCollectors(
            payroll: context.Payroll,
            payrunJob: context.PayrunJob,
            processorRegulation: processorRegulation);

        // derived wage types
        context.DerivedWageTypes = await GetDerivedWageTypes(
            payroll: context.Payroll,
            payrunJob: context.PayrunJob,
            processorRegulation: processorRegulation);
        if (!context.DerivedWageTypes.Any())
        {
            return await AbortJobAsync(context.PayrunJob, $"No wage types available for payrun with id {Payrun}");
        }

        // employees
        var employees = setup.Employees ?? await SetupEmployeesAsync(context, jobInvocation.EmployeeIdentifiers);
        if (employees.Count == 0)
        {
            return await AbortJobAsync(context.PayrunJob, $"No employees available for payrun with id {Payrun}");
        }
        Log.Trace($"Payrun with {employees.Count} employees");
        foreach (var employee in employees)
        {
            context.PayrunJob.Employees.Add(new() { EmployeeId = employee.Id });
        }

        // start payrun job
        context.PayrunJob.JobStart = Date.Now;
        context.PayrunJob.Message =
            $"Started payrun calculation with payroll {payrollId} on period {context.PayrunJob.PeriodName} for cycle {context.PayrunJob.CycleName}";
        Log.Debug(context.PayrunJob.Message);

        // Create new job or update existing pre-created job
        if (context.PayrunJob.Id == 0)
        {
            // New job (sync mode or retro jobs) - insert
            await Settings.PayrunJobRepository.CreateAsync(
                context: Settings.DbContext,
                parentId: Tenant.Id,
                item: context.PayrunJob);
        }
        else
        {
            // Pre-created job (async mode) - update
            await Settings.PayrunJobRepository.UpdateAsync(
                context: Settings.DbContext,
                parentId: Tenant.Id,
                item: context.PayrunJob);
        }

        // validate payroll regulations
        var validation = await new PayrollValidator(Settings.PayrollRepository).ValidateRegulations(
            context: Settings.DbContext,
            tenantId: Tenant.Id,
            payroll: context.Payroll,
            regulationDate: context.PayrunJob.PeriodEnd,
            evaluationDate: context.PayrunJob.EvaluationDate);
        if (!string.IsNullOrWhiteSpace(validation))
        {
            return await AbortJobAsync(context.PayrunJob, $"Payroll validation error: {validation}");
        }

        // job processing
        var payrunJob = await ProcessAllEmployeesAsync(
            setup: setup,
            context: context,
            processorRegulation: processorRegulation,
            processorRepositories: processorRepositories,
            employees: employees);
        return payrunJob;
    }

    private async Task<ILookup<string, DerivedCollector>> GetDerivedCollectors(Payroll payroll,
        PayrunJob payrunJob, PayrunProcessorRegulation processorRegulation)
    {
        // performance measure
        System.Diagnostics.Stopwatch stopwatch = null;
        if (LogWatch)
        {
            stopwatch = new();
            stopwatch.Start();
        }
        // context derived collectors grouped by collector name
        var collectorCluster = payroll.ClusterSetCollector;
        if (payrunJob.IsRetroJob && !string.IsNullOrWhiteSpace(payroll.ClusterSetCollectorRetro))
        {
            // retro job override
            collectorCluster = payroll.ClusterSetCollectorRetro;
        }
        var clusterSetCollector = payroll.ClusterSets?.FirstOrDefault(x => string.Equals(collectorCluster, x.Name));
        var derivedCollectors = await processorRegulation.GetDerivedCollectorsAsync(payrunJob, clusterSetCollector);

        Log.Trace($"{Payrun} with {derivedCollectors.Count} collectors");

        if (stopwatch != null)
        {
            stopwatch.Stop();
            Log.Debug($"{Payrun} load derived collectors [{derivedCollectors.Count}]: {stopwatch.ElapsedMilliseconds} ms");
            stopwatch.Restart();
        }

        return derivedCollectors;
    }

    private async Task<ILookup<decimal, DerivedWageType>> GetDerivedWageTypes(Payroll payroll, PayrunJob payrunJob,
        PayrunProcessorRegulation processorRegulation)
    {
        // performance measure
        System.Diagnostics.Stopwatch stopwatch = null;
        if (LogWatch)
        {
            stopwatch = new();
            stopwatch.Start();
        }

        // context derived wage types grouped by wage type identifier
        var wageTypeCluster = payroll.ClusterSetWageType;
        if (payrunJob.IsRetroJob && !string.IsNullOrWhiteSpace(payroll.ClusterSetWageTypeRetro))
        {
            // retro job override
            wageTypeCluster = payroll.ClusterSetWageTypeRetro;
        }
        var clusterSetWageType = payroll.ClusterSets?.FirstOrDefault(x => string.Equals(wageTypeCluster, x.Name));
        var derivedWageTypes = await processorRegulation.GetDerivedWageTypesAsync(payrunJob, clusterSetWageType);

        Log.Trace($"Payrun with {derivedWageTypes.Count} wage types");
        if (stopwatch != null)
        {
            stopwatch.Stop();
            Log.Debug($"{Payrun} load derived wage types [{derivedWageTypes.Count}]: {stopwatch.ElapsedMilliseconds} ms");
        }

        return derivedWageTypes;
    }

    private async Task<PayrunJob> ProcessAllEmployeesAsync(PayrunSetup setup, PayrunContext context,
        PayrunProcessorRegulation processorRegulation, PayrunProcessorRepositories processorRepositories, IList<Employee> employees)
    {
        try
        {
            // payroll validation
            var validation = await processorRepositories.ValidatePayrollAsync(context.Payroll, context.Division,
                context.PayrunJob.GetEvaluationPeriod(), context.EvaluationDate);
            if (validation != null)
            {
                return await AbortJobAsync(context.PayrunJob, $"Payrun validation error: {validation}");
            }

            // performance measure
            System.Diagnostics.Stopwatch stopwatch = null;
            if (LogWatch)
            {
                stopwatch = new();
                stopwatch.Start();
            }

            // process scripts
            var processorScript = new PayrunProcessorScripts(
                functionHost: FunctionHost,
                settings: Settings,
                regulationProvider: context,
                resultProvider: ResultProvider,
                tenant: Tenant,
                payrun: Payrun);
            // payrun start script
            if (!processorScript.PayrunStart(context))
            {
                return await AbortJobAsync(context.PayrunJob, $"Payrun start failed for payrun with id {Payrun}");
            }

            if (stopwatch != null)
            {
                stopwatch.Stop();
                Log.Debug($"{Payrun} load collectors [{context.DerivedCollectors.Count}]: {stopwatch.ElapsedMilliseconds} ms");
                stopwatch.Restart();
            }

            // update job with the total employee count
            context.PayrunJob.TotalEmployeeCount = employees.Count;
            await UpdateJobAsync(context.PayrunJob);

            // calculate payrun per employee
            foreach (var employee in employees)
            {
                try
                {
                    await ProcessEmployeeAsync(processorRegulation, processorScript, employee, context, setup);
                }
                catch (Exception exception)
                {
                    Log.Error(exception, exception.GetBaseMessage());
                    context.Errors.Add(employee, exception);
                }

                // update payrun job
                try
                {
                    context.PayrunJob.ProcessedEmployeeCount++;
                    Log.Trace($"Payrun calculated {context.PayrunJob.ProcessedEmployeeCount} employees");
                    await UpdateJobAsync(context.PayrunJob);
                }
                catch (Exception exception)
                {
                    Log.Error(exception, exception.GetBaseMessage());
                    context.Errors.Add(employee, exception);
                    // continue with the next employee
                }
            }

            // completed with failure(s)
            if (context.Errors.Any())
            {
                return await AbortJobAsync(context.PayrunJob, context.GetErrorMessages());
            }

            // payrun end script
            processorScript.PayrunEnd(context);

            // payrun job
            return context.PayrunJob;
        }
        catch (Exception exception)
        {
            var message = exception.GetBaseMessage();
            Log.Error(exception, message);
            return await AbortJobAsync(context.PayrunJob, $"Error in payrun: {message}");
        }
    }

    private async Task ProcessEmployeeAsync(PayrunProcessorRegulation processorRegulation, PayrunProcessorScripts processorScripts,
        Employee employee, PayrunContext context, PayrunSetup setup)
    {
        var retroJobs = new List<PayrunJob>();

        Log.Trace($"Payrun processing employee {employee}");
        try
        {
            var employeeCaseValueSet = setup.EmployeeCaseValues ??
                                       new CaseValueCache(Settings.DbContext, Settings.EmployeeCaseValueRepository,
                                           employee.Id, context.Division.Id, context.EvaluationDate, context.PayrunJob.Forecast);

            // value provider
            var caseValueProvider = new CaseValueProvider(employee,
                globalCaseValueRepository: context.GlobalCaseValues,
                nationalCaseValueRepository: context.NationalCaseValues,
                companyCaseValueRepository: context.CompanyCaseValues,
                employeeCaseValueRepository: employeeCaseValueSet,
                new()
                {
                    DbContext = Settings.DbContext,
                    Calculator = context.Calculator,
                    CaseFieldProvider = context.CaseFieldProvider,
                    EvaluationPeriod = context.EvaluationPeriod,
                    EvaluationDate = context.EvaluationDate,
                    RetroDate = context.RetroDate
                });

            // employee start function
            if (!processorScripts.EmployeeStart(caseValueProvider, context))
            {
                return;
            }

            // current period results
            // -> execution phase setup
            var employeeResults = await CalculateEmployeeAsync(processorRegulation, employee,
                caseValueProvider, context, PayrunExecutionPhase.Setup);
            var payrollResult = employeeResults.Item1;

            if (!payrollResult.CollectorResults.Any() && !payrollResult.WageTypeResults.Any())
            {
                context.PayrunJob.Message = "No results available";
                Log.Debug($"Payrun job {context.PayrunJob.Name} without results");
                return;
            }

            // payrun retro jobs
            var payrunRetroJobs = employeeResults.Item2;
            var retroDate = employeeResults.Item3?.Start;
            if (context.PayrunJob.RetroPayMode != RetroPayMode.None &&
                (retroDate != null || payrunRetroJobs != null && payrunRetroJobs.Any()))
            {
                var currentJob = context.PayrunJob;

                // target retro date
                if (payrunRetroJobs.Any())
                {
                    foreach (var payrunRetroJob in payrunRetroJobs)
                    {
                        if (retroDate == null || payrunRetroJob.ScheduleDate < retroDate.Value)
                        {
                            // retro date by script (manual)
                            retroDate = payrunRetroJob.ScheduleDate;
                        }
                    }
                }

                // retro job restrictions
                if (retroDate.HasValue)
                {
                    switch (Payrun.RetroTimeType)
                    {
                        case RetroTimeType.Anytime:
                            // unlimited retro calculation
                            break;
                        case RetroTimeType.Cycle:
                            // retro jobs limited to the payrun cycle
                            if (retroDate > currentJob.CycleStart)
                            {
                                retroDate = currentJob.CycleStart;
                            }
                            break;
                    }
                }

                // process retro payrun jobs
                if (retroDate.HasValue)
                {
                    // repeat until previous period
                    var retroPeriod = context.Calculator.GetPayrunPeriod(retroDate.Value);
                    while (retroPeriod != null && retroPeriod.Start < context.EvaluationPeriod.Start)
                    {
                        // retro payrun processor
                        var retroProcessor = new PayrunProcessor(Tenant, Payrun, Settings);

                        // retro payrun job
                        var retroJobInvocation = new PayrunJobInvocation
                        {
                            ParentJobId = currentJob.Id,
                            PayrunId = currentJob.PayrunId,
                            UserId = currentJob.CreatedUserId,
                            Name = currentJob.Name,
                            Owner = currentJob.Owner,
                            Forecast = currentJob.Forecast,
                            // retro jobs
                            RetroJobs = payrunRetroJobs,
                            // ensure no recursive retro pay calculation
                            RetroPayMode = RetroPayMode.None,
                            // incremental results only for retro pay run jobs
                            JobResult = PayrunJobResult.Incremental,
                            // retro payrun job status is complete
                            JobStatus = PayrunJobStatus.Complete,
                            PeriodStart = retroPeriod.Start,
                            EvaluationDate = context.EvaluationDate,
                            Reason = currentJob.CreatedReason,
                            StoreEmptyResults = context.StoreEmptyResults,
                            // current employee only
                            EmployeeIdentifiers = [employee.Identifier],
                            // consider runtime attribute changes
                            Attributes = currentJob.Attributes,
                        };

                        // setup
                        var payrunSetup = new PayrunSetup
                        {
                            Payroll = context.Payroll,
                            Division = context.Division,
                            Employees = [employee],
                            GlobalCaseValues = context.GlobalCaseValues,
                            NationalCaseValues = context.NationalCaseValues,
                            CompanyCaseValues = context.CompanyCaseValues,
                            EmployeeCaseValues = employeeCaseValueSet
                        };

                        // process retro payrun job (one level recursive)
                        var retroJob = await retroProcessor.Process(retroJobInvocation, payrunSetup);
                        retroJobs.Add(retroJob);

                        // complete retro job
                        await CompleteRetroJobAsync(retroJob);

                        // prepare period for the following retro payrun job
                        retroPeriod = retroPeriod.GetPayrollPeriod(retroPeriod.Start, 1);
                    }
                }

                // retro jobs may changed base calculation values: recalculate the current period
                // -> execution phase reevaluations
                if (retroJobs.Count > 0)
                {
                    // reset employee runtime values
                    context.RuntimeValueProvider.EmployeeValues.Clear();

                    // reevaluate current period results
                    employeeResults = await CalculateEmployeeAsync(processorRegulation, employee,
                        caseValueProvider, context, PayrunExecutionPhase.Reevaluation);
                    payrollResult = employeeResults.Item1;
                }
            }

            // result tags for retro jobs
            if (context.RetroPayrunJobs != null && context.RetroPayrunJobs.Any())
            {
                foreach (var retroJob in context.RetroPayrunJobs)
                {
                    // only retro jobs from the current period or
                    // retro jobs between the retro parent job period and the current job period
                    if (context.EvaluationPeriod.IsWithinOrAfter(retroJob.ScheduleDate))
                    {
                        payrollResult.AddTags(retroJob.ResultTags);
                    }
                }
            }

            // performance measure
            System.Diagnostics.Stopwatch stopwatch = null;
            if (LogWatch)
            {
                stopwatch = new();
                stopwatch.Start();
            }

            // store current period results by payrun job and employee
            if (context.StoreEmptyResults || !payrollResult.IsEmpty())
            {
                await Settings.PayrollResultSetRepository.CreateAsync(Settings.DbContext, Tenant.Id, payrollResult);
            }

            if (stopwatch != null)
            {
                stopwatch.Stop();
                Log.Debug($"{Payrun} store results: {stopwatch.ElapsedMilliseconds} ms");
            }

            // employee end function
            processorScripts.EmployeeEnd(caseValueProvider, context);
        }
        catch
        {
            // retro job cleanup
            foreach (var retroJob in retroJobs)
            {
                try
                {
                    Log.Trace($"Retro payrun job {retroJob.Name}: cleanup of employee {employee.Identifier} for period {retroJob.PeriodStart}");

                    // cleanup retro payrun job
                    await Settings.PayrunJobRepository.DeleteAsync(Settings.DbContext, Tenant.Id, retroJob.Id);
                    retroJob.ErrorMessage = $"Retro Payrun job {retroJob.Name}: error in parent Payrun {context.PayrunJob.Name}";
                }
                catch (Exception exception)
                {
                    Log.Error(exception.GetBaseMessage(), exception);
                    retroJob.ErrorMessage =
                        $"Retro Payrun job {retroJob.Name}: error in cleanup of employee {employee.Identifier} for period {retroJob.PeriodStart}: {exception.GetBaseMessage()}";
                }

                // update retro payrun job message
                try
                {
                    await UpdateJobAsync(retroJob);
                }
                catch (Exception exception)
                {
                    // log only
                    Log.Error(exception.GetBaseMessage(), exception);
                }
            }

            throw;
        }
    }

    /// <summary>
    /// Calculate single employee payrun
    /// </summary>
    /// <param name="processorRegulation">The processor regulation</param>
    /// <param name="employee">The employee</param>
    /// <param name="caseValueProvider">The case value provider</param>
    /// <param name="context">The payrun context</param>
    /// <param name="executionPhase">The job execution phase</param>
    /// <returns>The payroll results: the retro payrun jobs and the retro case value</returns>
    private async Task<Tuple<PayrollResultSet, List<RetroPayrunJob>, CaseValue>> CalculateEmployeeAsync(PayrunProcessorRegulation processorRegulation,
        Employee employee, ICaseValueProvider caseValueProvider, PayrunContext context, PayrunExecutionPhase executionPhase)
    {
        // context execution
        context.ExecutionPhase = executionPhase;

        // [culture by priority]: employee > division > tenant > system</remarks>
        var culture =
            // priority 1: employee culture
            employee.Culture ??
            // priority 2: division culture
            context.Division.Culture ??
            // priority 3: tenant culture
            Tenant.Culture ??
            // priority 4: system culture
            CultureInfo.CurrentCulture.Name;
        context.PushPayrollCulture(culture);

        // [calendar by priority]: employee > division > tenant</remarks>
        var calendarName =
            // priority 1: employee calendar
            employee.Calendar ??
            // priority 2: division calendar
            context.Division.Calendar ??
            // priority 3: tenant calendar
            Tenant.Calendar;
        context.CalendarName = calendarName;

        // payroll calculator based on the employee culture and calendar
        var employeeCalculator = await GetCalculatorAsync(Tenant.Id, context.User.Id, context.PayrollCulture, context.CalendarName);
        var prevCalculator = context.Calculator;
        context.Calculator = employeeCalculator;
        caseValueProvider.PushCalculator(employeeCalculator);

        // payroll results per employee and division
        var payrollResult = new PayrollResultSet
        {
            PayrollId = context.PayrunJob.PayrollId,
            PayrunId = Payrun.Id,
            PayrunJobId = context.PayrunJob.Id,
            EmployeeId = employee.Id,
            DivisionId = context.Division.Id,
            CycleName = context.PayrunJob.CycleName,
            CycleStart = context.PayrunJob.CycleStart,
            CycleEnd = context.PayrunJob.CycleEnd,
            PeriodName = context.PayrunJob.PeriodName,
            PeriodStart = context.PayrunJob.PeriodStart,
            PeriodEnd = context.PayrunJob.PeriodEnd
        };

        // collectors results
        SetupEmployeeCollectors(context, payrollResult);

        // retro jobs
        var retroPayrunJobs = new List<RetroPayrunJob>();

        // collector start
        foreach (var derivedCollector in context.DerivedCollectors)
        {
            Log.Trace($"Starting collector {derivedCollector}");
            var collectorResult =
                payrollResult.CollectorResults.First(x => string.Equals(x.CollectorName, derivedCollector.Key));
            var retroJobs = processorRegulation.CollectorStart(context, derivedCollector, caseValueProvider, payrollResult, collectorResult);
            collectorResult.Value = derivedCollector.First().Result;

            // retro payrun jobs
            AddRetroPayrunJobs(retroPayrunJobs, retroJobs, context.EvaluationPeriod.Start);
        }

        // performance measure
        System.Diagnostics.Stopwatch stopwatch = null;
        if (LogWatch)
        {
            stopwatch = new();
            stopwatch.Start();
        }
        System.Diagnostics.Stopwatch wageTypeStopwatch = null;
        if (stopwatch != null)
        {
            wageTypeStopwatch = new();
            wageTypeStopwatch.Start();
        }

        // executions (limited by system spec)
        var executionCount = 0;
        bool executionRestart;
        do
        {
            // next execution
            executionRestart = false;
            executionCount++;

            // wage types
            foreach (var derivedWageType in context.DerivedWageTypes)
            {
                // use the most derived (leaf) wage type
                Log.Trace($"Payrun processing wage type {derivedWageType.Key} on employee {employee}");

                IPayrollCalculator wageTypeCalculator = null;
                var prevWageTypeCalculator = context.Calculator;
                try
                {
                    // activate optional wage type calculator
                    var mostDerivedWageType = derivedWageType.First();
                    if (!string.IsNullOrWhiteSpace(mostDerivedWageType.Calendar) &&
                        !string.Equals(mostDerivedWageType.Calendar, context.CalendarName))
                    {
                        wageTypeCalculator = await GetCalculatorAsync(Tenant.Id, context.User.Id,
                            context.PayrollCulture, mostDerivedWageType.Calendar);
                        caseValueProvider.PushCalculator(wageTypeCalculator);
                        context.Calculator = wageTypeCalculator;
                    }

                    // payrun wage type available
                    if (!processorRegulation.IsWageTypeAvailable(context, derivedWageType, caseValueProvider))
                    {
                        Log.Trace($"Ignoring employee {employee} on wage type {derivedWageType.Key}");
                        continue;
                    }

                    // calculate result
                    var valueResult = processorRegulation.CalculateWageTypeValue(context, derivedWageType,
                            payrollResult, caseValueProvider, executionCount);

                    // restart execution
                    if (valueResult != null && valueResult.Item4)
                    {
                        // reset previous results
                        ResetEmployeeResults(context, payrollResult);
                        // restart the wage type calculation
                        executionRestart = true;
                        // exit wage type loop
                        break;
                    }

                    // wage type result
                    var wageTypeResult = valueResult?.Item1;
                    if (wageTypeResult != null)
                    {
                        payrollResult.WageTypeResults ??= [];
                        payrollResult.WageTypeResults.Add(wageTypeResult);

                        // retro payrun jobs
                        AddRetroPayrunJobs(retroPayrunJobs, valueResult.Item2, context.EvaluationPeriod.Start);

                        // collector apply
                        foreach (var derivedCollector in context.DerivedCollectors)
                        {
                            Log.Trace($"Payrun processing collector {derivedCollector.Key}");

                            // disabled collectors
                            if (valueResult.Item3.Contains(derivedCollector.Key))
                            {
                                continue;
                            }

                            // test if collector is available
                            if (PayrunProcessorRegulation.IsCollectorAvailable(derivedWageType, derivedCollector))
                            {
                                // calculate and update collector
                                var collectorResult =
                                    payrollResult.CollectorResults.First(x => string.Equals(x.CollectorName, derivedCollector.Key));
                                var applyResult = processorRegulation.CollectorApply(context, derivedCollector, caseValueProvider,
                                    wageTypeResult, payrollResult, collectorResult);
                                collectorResult.Value = applyResult.Item1;

                                // retro payrun jobs
                                AddRetroPayrunJobs(retroPayrunJobs, applyResult.Item2, context.EvaluationPeriod.Start);
                            }
                        }
                    }
                    else
                    {
                        Log.Trace($"Wage type {derivedWageType.Key} without result");
                    }
                }
                finally
                {
                    // restore payroll calculator
                    if (wageTypeCalculator != null)
                    {
                        caseValueProvider.PopCalculator(wageTypeCalculator);
                        context.Calculator = prevWageTypeCalculator;
                    }
                }

                if (wageTypeStopwatch != null)
                {
                    wageTypeStopwatch.Stop();
                    if (wageTypeStopwatch.Elapsed > Settings.FunctionLogTimeout)
                    {
                        Log.Debug($"{Payrun} calc wage type {derivedWageType.Key:##.#}: {wageTypeStopwatch.ElapsedMilliseconds} ms");
                    }

                    // next wage type
                    wageTypeStopwatch.Restart();
                }
            }
        } while (executionRestart && executionCount < SystemSpecification.PayrunMaxExecutionCount);

        if (stopwatch != null)
        {
            stopwatch.Stop();
            Log.Debug($"{Payrun} calc all wage types [{context.DerivedWageTypes.Count}]: {stopwatch.ElapsedMilliseconds} ms");
        }

        // collector end
        foreach (var derivedCollector in context.DerivedCollectors)
        {
            Log.Trace($"Ending collector {derivedCollector}");
            var collectorResult =
                payrollResult.CollectorResults.First(x => string.Equals(x.CollectorName, derivedCollector.Key));
            var retroJobs = processorRegulation.CollectorEnd(context, derivedCollector, caseValueProvider, payrollResult, collectorResult);
            collectorResult.Value = derivedCollector.First().Result;

            // retro payrun jobs
            AddRetroPayrunJobs(retroPayrunJobs, retroJobs, context.EvaluationPeriod.Start);
        }

        // get payrun results
        stopwatch?.Restart();

        // case values as payrun results (with enabled slots)
        payrollResult.PayrunResults.AddRange(
            await processorRegulation.GetCaseValuePayrunResultsAsync(
                payroll: context.Payroll,
                payrunJob: context.PayrunJob,
                caseValueProvider: caseValueProvider,
                culture: culture,
                expandCaseSlots: true));

        if (stopwatch != null)
        {
            stopwatch.Stop();
            Log.Debug($"{Payrun} get payrun results [{payrollResult.PayrunResults.Count}]: {stopwatch.ElapsedMilliseconds} ms");
        }

        // incremental mode: remove unchanged results
        if (context.PayrunJob.JobResult == PayrunJobResult.Incremental)
        {
            await RemoveUnchangedResultsAsync(Tenant.Id, payrollResult, context.EvaluationDate);
        }

        // remove employee calculator
        caseValueProvider.PopCalculator(employeeCalculator);
        context.Calculator = prevCalculator;

        // restore culture
        context.PopPayrollCulture(culture);

        // set results creation date
        payrollResult.SetResultDate(context.EvaluationDate);

        return new(payrollResult, retroPayrunJobs, caseValueProvider.RetroCaseValue);
    }

    private static void ResetEmployeeResults(PayrunContext context, PayrollResultSet payrollResult)
    {
        // reset collectors
        SetupEmployeeCollectors(context, payrollResult);

        // reset wage type results
        payrollResult.WageTypeResults?.Clear();

        // reset payrun results
        payrollResult.PayrunResults?.Clear();
    }

    private static void SetupEmployeeCollectors(PayrunContext context, PayrollResultSet payrollResult)
    {

        // ensure clean collector results
        if (payrollResult.CollectorResults != null)
        {
            payrollResult.CollectorResults.Clear();
        }
        else
        {
            payrollResult.CollectorResults = [];
        }

        foreach (var derivedCollector in context.DerivedCollectors)
        {
            // reset values of previous employee
            foreach (var collector in derivedCollector)
            {
                collector.Reset();
            }

            // add single result
            var resultCollector = derivedCollector.First();
            payrollResult.CollectorResults.Add(new()
            {
                CollectorId = resultCollector.Id,
                CollectorName = resultCollector.Name,
                CollectorNameLocalizations = resultCollector.NameLocalizations,
                CollectMode = resultCollector.CollectMode,
                Negated = resultCollector.Negated,
                ValueType = resultCollector.ValueType,
                Culture = resultCollector.Culture ?? context.PayrollCulture,
                Start = context.EvaluationPeriod.Start,
                End = context.EvaluationPeriod.End,
                Attributes = new(),
                CustomResults = []
            });
        }
    }

    #endregion

    #region Employees

    private async Task<List<Employee>> SetupEmployeesAsync(PayrunContext context, List<string> employeeIdentifiers)
    {
        List<Employee> employees;

        // selected employees by identifier
        if (employeeIdentifiers != null && employeeIdentifiers.Any())
        {
            employees = [];
            foreach (var employeeIdentifier in employeeIdentifiers)
            {
                var query = new DivisionQuery
                {
                    Status = ObjectStatus.Active,
                    DivisionId = context.Division.Id,
                    Filter = $"{nameof(Employee.Identifier)} eq '{employeeIdentifier}'"
                };
                var selectedEmployees = (await Settings.EmployeeRepository.QueryAsync(Settings.DbContext, Tenant.Id, query)).ToList();
                if (selectedEmployees.Count != 1)
                {
                    throw new PayrunException($"Unknown employee with identifier {employeeIdentifier}");
                }
                var employee = selectedEmployees.First();
                // status
                if (employee.Status != ObjectStatus.Active)
                {
                    throw new PayrunException(
                        $"Payrun on inactive employee with identifier {employeeIdentifier} ({employee.Status})");
                }
                // division
                if (!employee.InDivision(context.Division.Name))
                {
                    throw new PayrunException(
                        $"Employee with identifier {employeeIdentifier} is not in division {context.Division.Name}");
                }

                employees.Add(employee);
            }

            return employees;
        }

        // all active employees from the division
        var allQuery = new DivisionQuery
        {
            Status = ObjectStatus.Active,
            DivisionId = context.Division.Id
        };
        employees = (await Settings.EmployeeRepository.QueryAsync(Settings.DbContext, Tenant.Id, allQuery)).ToList();

        // employee available expression
        if (employees.Any() && !string.IsNullOrWhiteSpace(Payrun.EmployeeAvailableExpression))
        {
            employees = FilterAvailableEmployees(context, employees);
        }

        Log.Trace($"Payrun with {employees.Count} employees");
        return employees;
    }

    private List<Employee> FilterAvailableEmployees(PayrunContext context, List<Employee> employees)
    {
        var availableEmployees = new List<Employee>();

        var scriptController = new PayrunScriptController();
        foreach (var employee in employees)
        {
            // employee case values
            var employeeCaseValueSet = new CaseValueCache(Settings.DbContext, Settings.EmployeeCaseValueRepository, employee.Id,
                context.PayrunJob.DivisionId, context.PayrunJob.EvaluationDate, context.PayrunJob.Forecast);

            var caseValueProvider = new CaseValueProvider(employee,
                globalCaseValueRepository: context.GlobalCaseValues,
                nationalCaseValueRepository: context.NationalCaseValues,
                companyCaseValueRepository: context.CompanyCaseValues,
                employeeCaseValueRepository: employeeCaseValueSet,
                new()
                {
                    DbContext = Settings.DbContext,
                    Calculator = context.Calculator,
                    CaseFieldProvider = context.CaseFieldProvider,
                    EvaluationPeriod = context.EvaluationPeriod,
                    EvaluationDate = context.EvaluationDate,
                    RetroDate = context.RetroDate,

                });
            var isAvailable = scriptController.IsEmployeeAvailable(new()
            {
                DbContext = Settings.DbContext,
                PayrollCulture = context.PayrollCulture,
                Namespace = null,
                FunctionHost = FunctionHost,
                Tenant = Tenant,
                User = context.User,
                Payroll = context.Payroll,
                Payrun = Payrun,
                PayrunJob = context.PayrunJob,
                ParentPayrunJob = context.ParentPayrunJob,
                ExecutionPhase = context.ExecutionPhase,
                RegulationProvider = context,
                ResultProvider = ResultProvider,
                CaseValueProvider = caseValueProvider,
                RegulationLookupProvider = context.RegulationLookupProvider,
                RuntimeValueProvider = context.RuntimeValueProvider,
                DivisionRepository = Settings.DivisionRepository,
                EmployeeRepository = Settings.EmployeeRepository,
                CalendarRepository = Settings.CalendarRepository,
                PayrollCalculatorProvider = Settings.PayrollCalculatorProvider,
                WebhookDispatchService = Settings.WebhookDispatchService
            });
            if (isAvailable.HasValue && isAvailable.Value)
            {
                availableEmployees.Add(employee);
            }
            else
            {
                Log.Trace($"Ignoring employee {employee}");
            }
        }

        return availableEmployees;
    }

    #endregion

    #region Retro

    private async Task<DateTime?> GetRetroDateAsync(PayrunJobInvocation jobInvocation)
    {
        if (jobInvocation.RetroPayMode == RetroPayMode.None)
        {
            // no retro pay support
            return null;
        }

        // previous jobs from the same payrun
        var query = QueryFactory.NewEqualFilterQuery(new Dictionary<string, object>
        {
            {nameof(PayrunJobInvocation.PayrunId), jobInvocation.PayrunId},
            // ignore incremental retro jobs
            {nameof(PayrunJob.JobResult), Enum.GetName(typeof(PayrunJobResult), PayrunJobResult.Full) }
        });
        var payrunJobs = (await Settings.PayrunJobRepository.QueryAsync(Settings.DbContext, Tenant.Id, query))
            // filter out previous periods
            .Where(x => x.PeriodStart < jobInvocation.PeriodStart)
            .ToList();
        if (!payrunJobs.Any())
        {
            return null;
        }

        // legal/forecast filter
        var availablePayrunJobs = string.IsNullOrWhiteSpace(jobInvocation.Forecast) ?
            // legal job: select legal jobs
            payrunJobs.Where(x => string.IsNullOrWhiteSpace(x.Forecast)).ToList() :
            // forecast job: select forecast jobs with the same name
            payrunJobs.Where(x => string.Equals(x.Forecast, jobInvocation.Forecast)).ToList();
        if (!availablePayrunJobs.Any())
        {
            return null;
        }

        // previous job
        var previousJob = availablePayrunJobs
            // first order criteria: evaluation date
            .OrderByDescending(x => x.EvaluationDate)
            // second order criteria: created date (for multiple jobs on the same payrun)
            .ThenByDescending(x => x.Created)
            .First();
        return previousJob.EvaluationDate.ToUtc();
    }

    private static void AddRetroPayrunJobs(List<RetroPayrunJob> payrunJobs, List<RetroPayrunJob> retroJobs, DateTime evaluationPeriodStart)
    {
        if (retroJobs != null && retroJobs.Any())
        {
            foreach (var payrunJob in retroJobs)
            {
                if (payrunJob.ScheduleDate > evaluationPeriodStart)
                {
                    throw new PayrollException(
                        $"Retro job schedule date {payrunJob.ScheduleDate} must be before {evaluationPeriodStart}.");
                }
                payrunJobs.Add(payrunJob);
            }
        }
    }

    #endregion

    #region Payroll Calculator

    // calculator is stored by calendar name
    private readonly Dictionary<string, IPayrollCalculator> payrollCalculators =
        new();

    private readonly Calendar defaultCalendar = new();

    private async Task<IPayrollCalculator> GetCalculatorAsync(int tenantId, int userId,
        string culture = null, string calendarName = null)
    {
        // calendar: first on payrun and second on tenant, otherwise use the default calendar
        var calendar = defaultCalendar;
        if (!string.IsNullOrWhiteSpace(calendarName))
        {
            calendar = await Settings.CalendarRepository.GetByNameAsync(Settings.DbContext, tenantId, calendarName);
            if (calendar == null)
            {
                throw new PayrollException($"Unknown calendar {calendarName}.");
            }
        }
        else
        {
            calendarName = ".default";
        }

        // cache
        if (payrollCalculators.TryGetValue(calendarName, out var payrollCalculator))
        {
            return payrollCalculator;
        }

        // culture
        var cultureInfo = culture == null ? CultureInfo.CurrentCulture : new(culture);

        // new calculator based on the tenant calendar configuration
        var calculator = Settings.PayrollCalculatorProvider.CreateCalculator(
            tenantId: tenantId,
            userId: userId,
            culture: cultureInfo,
            calendar: calendar);
        payrollCalculators.Add(calendarName, calculator);
        return calculator;
    }

    #endregion

    #region Job

    private async Task UpdateJobAsync(PayrunJob payrunJob) =>
        await Settings.PayrunJobRepository.UpdateAsync(Settings.DbContext, Tenant.Id, payrunJob);

    private async Task<PayrunJob> AbortJobAsync(PayrunJob payrunJob, string message, Exception error = null)
    {
        // setup
        payrunJob.JobStatus = PayrunJobStatus.Abort;
        payrunJob.JobEnd = Date.Now;
        payrunJob.Message = message;
        if (error != null)
        {
            Log.Error(error, message);
        }
        else
        {
            Log.Error(message);
        }
        payrunJob.ErrorMessage = error?.ToString();

        // persist
        await Settings.PayrunJobRepository.UpdateAsync(Settings.DbContext, Tenant.Id, payrunJob);

        return payrunJob;
    }

    private async Task CompleteRetroJobAsync(PayrunJob payrunJob)
    {
        // setup
        payrunJob.JobStatus = PayrunJobStatus.Complete;
        payrunJob.JobEnd = Date.Now;
        var duration = payrunJob.JobEnd.Value - payrunJob.JobStart;
        payrunJob.Message = $"Completed retro payrun calculation successfully in {duration.ToReadableString()}.";
        Log.Debug(payrunJob.Message);

        // persist
        await Settings.PayrunJobRepository.UpdateAsync(Settings.DbContext, Tenant.Id, payrunJob);
    }

    #endregion

    #region Results

    private async Task RemoveUnchangedResultsAsync(int tenantId, PayrollResultSet payrollResult, DateTime evaluationDate)
    {
        // existing collectors by collector name
        var collectorResults = (await Settings.PayrollConsolidatedResultRepository.GetCollectorResultsAsync(Settings.DbContext,
            new()
            {
                TenantId = tenantId,
                EmployeeId = payrollResult.EmployeeId,
                DivisionId = payrollResult.DivisionId,
                PeriodStarts = new List<DateTime> { payrollResult.PeriodStart },
                EvaluationDate = evaluationDate
            })).ToList();
        if (collectorResults.Any())
        {
            foreach (var collectorResult in collectorResults)
            {
                var currentResult = payrollResult.CollectorResults.FirstOrDefault(
                    x => string.Equals(x.CollectorName, collectorResult.CollectorName));
                if (currentResult != null && currentResult.Value == collectorResult.Value)
                {
                    var remove = currentResult.Value == collectorResult.Value;
                    // special case tag only change: reset the value change
                    if (remove && !CompareTool.EqualLists(currentResult.Tags, collectorResult.Tags))
                    {
                        // keep tag result
                        remove = false;
                    }
                    if (remove)
                    {
                        payrollResult.CollectorResults.Remove(currentResult);
                    }
                }
            }
        }

        // existing wage types by wage type number
        var wageTypeResults = (await Settings.PayrollConsolidatedResultRepository.GetWageTypeResultsAsync(Settings.DbContext,
            new()
            {
                TenantId = tenantId,
                EmployeeId = payrollResult.EmployeeId,
                DivisionId = payrollResult.DivisionId,
                PeriodStarts = new List<DateTime> { payrollResult.PeriodStart },
                EvaluationDate = evaluationDate
            })).ToList();
        if (wageTypeResults.Any())
        {
            foreach (var wageTypeResult in wageTypeResults)
            {
                var currentResult = payrollResult.WageTypeResults.FirstOrDefault(
                    x => x.WageTypeNumber == wageTypeResult.WageTypeNumber);
                if (currentResult != null)
                {
                    var remove = currentResult.Value == wageTypeResult.Value;
                    // special case tag only change: reset the value change
                    if (remove && !CompareTool.EqualDistinctLists(currentResult.Tags, wageTypeResult.Tags))
                    {
                        // keep tag result
                        remove = false;
                    }
                    if (remove)
                    {
                        payrollResult.WageTypeResults.Remove(currentResult);
                    }
                }
            }
        }
    }

    #endregion

}
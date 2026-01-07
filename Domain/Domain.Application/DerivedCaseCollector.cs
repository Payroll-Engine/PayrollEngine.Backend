using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting.Controller;
using PayrollEngine.Domain.Scripting.Runtime;

namespace PayrollEngine.Domain.Application;

public class DerivedCaseCollector : DerivedCaseTool
{
    /// <summary>
    /// Constructor for global cases
    /// </summary>
    public DerivedCaseCollector(
        IGlobalCaseValueRepository globalCaseValueRepository,
        DerivedCaseToolSettings settings) :
        base(globalCaseValueRepository, settings)
    {
    }

    /// <summary>
    /// Constructor for national cases
    /// </summary>
    public DerivedCaseCollector(
        IGlobalCaseValueRepository globalCaseValueRepository,
        INationalCaseValueRepository nationalCaseValueRepository,
        DerivedCaseToolSettings settings) :
        base(globalCaseValueRepository, nationalCaseValueRepository, settings)
    {
    }

    /// <summary>
    /// Constructor for company cases
    /// </summary>
    public DerivedCaseCollector(
        IGlobalCaseValueRepository globalCaseValueRepository,
        INationalCaseValueRepository nationalCaseValueRepository,
        ICompanyCaseValueRepository companyCaseValueRepository,
        DerivedCaseToolSettings settings) :
        base(globalCaseValueRepository, nationalCaseValueRepository, companyCaseValueRepository,
            settings)
    {
    }

    /// <summary>
    /// Constructor for employee cases
    /// </summary>
    public DerivedCaseCollector(Employee employee,
        IGlobalCaseValueRepository globalCaseValueRepository,
        INationalCaseValueRepository nationalCaseValueRepository,
        ICompanyCaseValueRepository companyCaseValueRepository,
        IEmployeeCaseValueRepository employeeCaseValueRepository,
        DerivedCaseToolSettings settings) :
        base(employee, globalCaseValueRepository, nationalCaseValueRepository, companyCaseValueRepository,
            employeeCaseValueRepository, settings)
    {
    }

    public async Task<bool> GlobalCaseAvailableAsync(string caseName, string culture) =>
        await CaseAvailableAsync(CaseType.Global, caseName, culture);

    public async Task<bool> NationalCaseAvailableAsync(string caseName, string culture) =>
        await CaseAvailableAsync(CaseType.National, caseName, culture);

    public async Task<bool> CompanyCaseAvailableAsync(string caseName, string culture) =>
        await CaseAvailableAsync(CaseType.Company, caseName, culture);

    public async Task<bool> EmployeeCaseAvailableAsync(string caseName, string culture) =>
        await CaseAvailableAsync(CaseType.Employee, caseName, culture);

    public async Task<IEnumerable<Case>> GetAvailableGlobalCasesAsync(string culture,
        IEnumerable<string> caseNames = null, bool? hidden = false) =>
        await GetAvailableCasesAsync(CaseType.Global, culture, caseNames, hidden);

    public async Task<IEnumerable<Case>> GetAvailableNationalCasesAsync(string culture,
        IEnumerable<string> caseNames = null, bool? hidden = false) =>
        await GetAvailableCasesAsync(CaseType.National, culture, caseNames, hidden);

    public async Task<IEnumerable<Case>> GetAvailableCompanyCasesAsync(string culture,
        IEnumerable<string> caseNames = null, bool? hidden = false) =>
        await GetAvailableCasesAsync(CaseType.Company, culture, caseNames, hidden);

    public async Task<IEnumerable<Case>> GetAvailableEmployeeCasesAsync(string culture,
        IEnumerable<string> caseNames = null, bool? hidden = false) =>
        await GetAvailableCasesAsync(CaseType.Employee, culture, caseNames, hidden);

    /// <summary>
    /// Get case period values by date period and the case field names
    /// </summary>
    /// <param name="period">The date period</param>
    /// <param name="caseFieldNames">The case field names</param>
    /// <returns>The case values for all case fields</returns>
    public async Task<IEnumerable<CaseFieldValue>> GetCasePeriodValuesAsync(
        DatePeriod period, IEnumerable<string> caseFieldNames) =>
        await CaseValueProvider.GetCasePeriodValuesAsync(period, caseFieldNames);

    /// <summary>
    /// Test if case is available
    /// </summary>
    /// <param name="caseType">Type of the case</param>
    /// <param name="caseName">Name of the case</param>
    /// <param name="culture">The culture</param>
    /// <returns>True if the case is available</returns>
    private async Task<bool> CaseAvailableAsync(CaseType caseType, string caseName, string culture)
    {
        if (string.IsNullOrWhiteSpace(caseName))
        {
            throw new ArgumentException(nameof(caseName));
        }

        // case (derived)
        var cases = (await PayrollRepository.GetDerivedCasesAsync(Settings.DbContext,
            new()
            {
                TenantId = Tenant.Id,
                PayrollId = Payroll.Id,
                RegulationDate = RegulationDate,
                EvaluationDate = EvaluationDate
            },
            caseType: caseType,
            caseNames: [caseName],
            clusterSet: ClusterSet,
            overrideType: OverrideType.Active)).ToList();
        var available = await CaseAvailable(cases, culture);
        Log.Trace(available ? $"Case {caseName} available" : $"Case {caseName} not available");
        return available;
    }

    /// <summary>
    /// Get available cases
    /// </summary>
    /// <param name="caseType">Type of the case</param>
    /// <param name="caseNames">The case names (default: all)</param>
    /// <param name="culture">The culture</param>
    /// <param name="hidden">Hidden cases (default: all)</param>
    /// <returns>List of available cases</returns>
    private async Task<IEnumerable<Case>> GetAvailableCasesAsync(CaseType caseType, string culture,
        IEnumerable<string> caseNames = null, bool? hidden = false)
    {
        var availableCases = new List<Case>();

        // case (derived)
        var allCases = (await PayrollRepository.GetDerivedCasesAsync(Settings.DbContext,
            new()
            {
                TenantId = Tenant.Id,
                PayrollId = Payroll.Id,
                RegulationDate = RegulationDate,
                EvaluationDate = EvaluationDate
            },
            caseType: caseType,
            caseNames: caseNames,
            clusterSet: ClusterSet,
            overrideType: OverrideType.Active,
            hidden: hidden)).ToList();
        if (allCases.Any())
        {
            // collect cases by case name
            var groupedCases = allCases.GroupBy(x => x.Name, y => y);
            foreach (var groupedCase in groupedCases)
            {
                var cases = groupedCase.Cast<Case>().ToList();

                // case available
                var available = await CaseAvailable(cases, culture);
                if (available)
                {
                    // collect derived case values
                    var caseSet = DerivedCaseFactory.BuildCase(cases, null, culture);
                    availableCases.Add(caseSet);
                    Log.Trace($"Case {groupedCase.Key} available");
                }
                else
                {
                    Log.Trace($"Case {groupedCase.Key} not available");
                }
            }
        }

        return availableCases;
    }

    private async Task<bool> CaseAvailable(IEnumerable<Case> cases, string culture)
    {
        var lookupProvider = NewRegulationLookupProvider();

        // case available expression
        var caseList = (cases as Case[] ?? cases.ToArray()).ToList();
        if (!caseList.Any())
        {
            return false;
        }

        var availableScripts = caseList.GetDerivedExpressionObjects(x => x.AvailableScript);
        if (availableScripts.Any())
        {
            // remove cases without available expression
            caseList = caseList.Where(x => !string.IsNullOrWhiteSpace(x.AvailableScript)).ToList();

            // case set
            var caseSet = await GetDerivedCaseSetAsync(caseList, null, null, culture, false);

            var settings = new CaseRuntimeSettings
            {
                DbContext = Settings.DbContext,
                UserCulture = culture,
                FunctionHost = FunctionHost,
                Tenant = Tenant,
                User = User,
                Payroll = Payroll,
                CaseValueProvider = CaseValueProvider,
                RegulationLookupProvider = lookupProvider,
                DivisionRepository = DivisionRepository,
                EmployeeRepository = EmployeeRepository,
                CalendarRepository = CalendarRepository,
                PayrollCalculatorProvider = PayrollCalculatorProvider,
                WebhookDispatchService = WebhookDispatchService,
                Case = caseSet
            };

            // case available function call
            foreach (var _ in availableScripts)
            {
                var available = new CaseScriptController().CaseAvailable(settings);
                if (available.HasValue)
                {
                    return available.Value;
                }
            }
        }
        return true;
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting;
using CaseValue = PayrollEngine.Domain.Model.CaseValue;

namespace PayrollEngine.Domain.Application;

public class CaseValueTool : FunctionToolBase
{
    private ICaseFieldProvider CaseFieldProvider { get; }
    private ICaseValueProvider CaseValueProvider { get; }

    private DateTime ValueDate { get; }
    private DateTime EvaluationDate { get; }

    private Tenant Tenant { get; }
    private Payroll Payroll { get; }
    private Employee Employee { get; }

    // settings
    private new CaseValueToolSettings Settings => base.Settings as CaseValueToolSettings;

    // repositories
    public IPayrollRepository PayrollRepository { get; }
    public ICaseRepository CaseRepository { get; }

    /// <summary>
    /// Constructor for global case value
    /// </summary>
    public CaseValueTool(
        IGlobalCaseValueRepository globalCaseValueRepository,
        CaseValueToolSettings settings)
        : this(settings)
    {
        // culture
        var culture = string.IsNullOrWhiteSpace(Tenant.Culture) ?
                CultureInfo.CurrentCulture :
                 new CultureInfo(Tenant.Culture);

        // value provider
        var calculator = settings.PayrollCalculatorProvider.CreateCalculator(Tenant.Id, culture: culture, calendar: settings.Calendar);
        CaseValueProvider = new CaseValueProvider(
            new CaseValueCache(settings.DbContext, globalCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new()
            {
                DbContext = Settings.DbContext,
                Calculator = calculator,
                CaseFieldProvider = CaseFieldProvider,
                EvaluationPeriod = calculator.GetPayrunPeriod(settings.EvaluationDate).GetDatePeriod(),
                EvaluationDate = settings.EvaluationDate
            });
    }

    /// <summary>
    /// Constructor for national case value
    /// </summary>
    public CaseValueTool(
        IGlobalCaseValueRepository globalCaseValueRepository,
        INationalCaseValueRepository nationalCaseValueRepository,
        CaseValueToolSettings settings)
        : this(settings)
    {
        // culture
        var culture = string.IsNullOrWhiteSpace(Tenant.Culture) ?
            CultureInfo.CurrentCulture :
            new CultureInfo(Tenant.Culture);

        // value provider
        var calculator = settings.PayrollCalculatorProvider.CreateCalculator(Tenant.Id, culture: culture, calendar: settings.Calendar);
        CaseValueProvider = new CaseValueProvider(
            new CaseValueCache(settings.DbContext, globalCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new CaseValueCache(settings.DbContext, nationalCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new()
            {
                DbContext = Settings.DbContext,
                Calculator = calculator,
                CaseFieldProvider = CaseFieldProvider,
                EvaluationPeriod = calculator.GetPayrunPeriod(settings.EvaluationDate).GetDatePeriod(),
                EvaluationDate = settings.EvaluationDate
            });
    }

    /// <summary>
    /// Constructor for company case value
    /// </summary>
    public CaseValueTool(
        IGlobalCaseValueRepository globalCaseValueRepository,
        INationalCaseValueRepository nationalCaseValueRepository,
        ICompanyCaseValueRepository companyCaseValueRepository,
        CaseValueToolSettings settings)
        : this(settings)
    {
        // culture
        var culture = string.IsNullOrWhiteSpace(Tenant.Culture) ?
            CultureInfo.CurrentCulture :
            new CultureInfo(Tenant.Culture);

        // value provider
        var calculator = settings.PayrollCalculatorProvider.CreateCalculator(Tenant.Id, culture: culture, calendar: settings.Calendar);
        CaseValueProvider = new CaseValueProvider(
            new CaseValueCache(settings.DbContext, globalCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new CaseValueCache(settings.DbContext, nationalCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new CaseValueCache(settings.DbContext, companyCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new()
            {
                DbContext = Settings.DbContext,
                Calculator = calculator,
                CaseFieldProvider = CaseFieldProvider,
                EvaluationPeriod = calculator.GetPayrunPeriod(settings.EvaluationDate).GetDatePeriod(),
                EvaluationDate = settings.EvaluationDate
            });
    }

    /// <summary>
    /// Constructor for employee case value
    /// </summary>
    public CaseValueTool(Employee employee,
        IGlobalCaseValueRepository globalCaseValueRepository,
        INationalCaseValueRepository nationalCaseValueRepository,
        ICompanyCaseValueRepository companyCaseValueRepository,
        IEmployeeCaseValueRepository employeeCaseValueRepository,
        CaseValueToolSettings settings)
        : this(settings)
    {
        Employee = employee ?? throw new ArgumentNullException(nameof(employee));

        // culture
        var culture = string.IsNullOrWhiteSpace(Tenant.Culture) ?
            CultureInfo.CurrentCulture :
            new CultureInfo(Tenant.Culture);

        // value provider
        var calculator = settings.PayrollCalculatorProvider.CreateCalculator(Tenant.Id, culture: culture, calendar: settings.Calendar);
        CaseValueProvider = new CaseValueProvider(Employee,
            new CaseValueCache(settings.DbContext, globalCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new CaseValueCache(settings.DbContext, nationalCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new CaseValueCache(settings.DbContext, companyCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new CaseValueCache(settings.DbContext, employeeCaseValueRepository, Employee.Id, Payroll.DivisionId, EvaluationDate),
            new()
            {
                DbContext = Settings.DbContext,
                Calculator = calculator,
                CaseFieldProvider = CaseFieldProvider,
                EvaluationPeriod = calculator.GetPayrunPeriod(settings.EvaluationDate).GetDatePeriod(),
                EvaluationDate = settings.EvaluationDate
            });
    }

    /// <summary>
    /// Internal ctor without case value repos
    /// </summary>
    private CaseValueTool(CaseValueToolSettings settings) :
        base(settings)
    {
        // global
        Tenant = settings.Tenant ?? throw new ArgumentNullException(nameof(settings.Tenant));
        Payroll = settings.Payroll ?? throw new ArgumentNullException(nameof(settings.Payroll));
        PayrollRepository = settings.PayrollRepository ?? throw new ArgumentNullException(nameof(PayrollRepository));

        ValueDate = settings.ValueDate;
        EvaluationDate = settings.EvaluationDate;

        CaseFieldProvider = new CaseFieldProvider(
            new CaseFieldProxyRepository(PayrollRepository, Tenant.Id, Payroll.Id, ValueDate, EvaluationDate));
        CaseRepository = settings.CaseRepository ?? throw new ArgumentNullException(nameof(CaseRepository));
    }

    /// <summary>
    /// Test if the case field is valid
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="caseFieldName">The name of the case field</param>
    /// <returns>The case value</returns>
    public async Task<bool> ValidCaseFieldAsync(IDbContext context, string caseFieldName) =>
        await CaseFieldProvider.GetCaseFieldAsync(context, caseFieldName) != null;

    /// <summary>
    /// Get case values (only active objects)
    /// </summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="evaluationPeriod">The evaluation period</param>
    /// <param name="caseSlot">The case slot</param>
    /// <returns>The case value periods for a time period</returns>
    public async Task<List<CaseFieldValue>> GetCaseValuesAsync(string caseFieldName, DatePeriod evaluationPeriod,
        string caseSlot = null) =>
        await CaseValueProvider.GetCaseValuesAsync(caseFieldName, evaluationPeriod, caseSlot);

    /// <summary>
    /// Get all case values (only active objects) by case type
    /// </summary>
    /// <param name="valueDate">The value date</param>
    /// <param name="caseType">The case type</param>
    /// <returns>The case value</returns>
    public async Task<List<CaseValue>> GetTimeCaseValuesAsync(DateTime valueDate, CaseType caseType) =>
        await CaseValueProvider.GetTimeCaseValuesAsync(valueDate, caseType);

    /// <summary>
    /// Get case values (only active objects) from a specific time
    /// </summary>
    /// <param name="valueDate">The value date</param>
    /// <param name="caseType">The case type</param>
    /// <param name="caseFieldNames">The case field names</param>
    /// <returns>The case values</returns>
    public async Task<List<CaseValue>> GetTimeCaseValuesAsync(DateTime valueDate, CaseType caseType,
        IEnumerable<string> caseFieldNames) =>
        await CaseValueProvider.GetTimeCaseValuesAsync(valueDate, caseType, caseFieldNames);
}
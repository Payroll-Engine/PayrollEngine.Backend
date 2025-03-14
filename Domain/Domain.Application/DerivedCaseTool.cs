﻿using System;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public abstract class DerivedCaseTool : FunctionToolBase
{
    // settings
    protected new DerivedCaseToolSettings Settings => base.Settings as DerivedCaseToolSettings;

    // providers
    protected ICaseProvider CaseProvider { get; }
    private ICaseFieldProvider CaseFieldProvider { get; }
    protected ICaseValueProvider CaseValueProvider { get; }

    /// <summary>
    /// The division repository
    /// </summary>
    protected IDivisionRepository DivisionRepository { get; }

    /// <summary>
    /// The employee repository
    /// </summary>
    protected IEmployeeRepository EmployeeRepository { get; }

    /// <summary>
    /// The calendar repository
    /// </summary>
    protected ICalendarRepository CalendarRepository { get; }

    /// <summary>
    /// The payroll calculator provider
    /// </summary>
    protected IPayrollCalculatorProvider PayrollCalculatorProvider { get; }

    /// <summary>
    /// The webhook dispatcher
    /// </summary>
    protected IWebhookDispatchService WebhookDispatchService { get; }

    /// <summary>
    /// The regulation date
    /// </summary>
    protected DateTime RegulationDate { get; }

    /// <summary>
    /// The evaluation date
    /// </summary>
    protected DateTime EvaluationDate { get; }

    /// <summary>
    /// The cluster set
    /// </summary>
    protected ClusterSet ClusterSet { get; }

    // global
    protected Tenant Tenant { get; }
    protected CultureInfo TenantCulture { get; }
    protected User User { get; }
    protected Payroll Payroll { get; }
    private Employee Employee { get; }

    // repositories
    protected IPayrollRepository PayrollRepository { get; }
    private IRegulationRepository RegulationRepository { get; }
    private ILookupSetRepository RegulationLookupSetRepository { get; }
    private IGlobalCaseValueRepository GlobalCaseValueRepository { get; }
    private INationalCaseValueRepository NationalCaseValueRepository { get; }
    private ICompanyCaseValueRepository CompanyCaseValueRepository { get; }
    private IEmployeeCaseValueRepository EmployeeCaseValueRepository { get; }

    /// <summary>
    /// Case culture
    /// <remarks>[culture by priority]: tool-setting > user > system</remarks>
    /// </summary>
    protected string UserCulture =>
        // priority 1: setting culture
        Settings.Culture ??
        // priority 2: user culture
        User.Culture ??
        // priority 3: system culture
        CultureInfo.CurrentCulture.Name;

    /// <summary>
    /// Constructor for derived global cases
    /// </summary>
    protected DerivedCaseTool(
        IGlobalCaseValueRepository globalCaseValueRepository,
        DerivedCaseToolSettings settings) :
        this(settings)
    {
        // repositories
        GlobalCaseValueRepository = globalCaseValueRepository ??
                                    throw new ArgumentNullException(nameof(globalCaseValueRepository));

        // local
        var calculator = settings.PayrollCalculatorProvider.CreateCalculator(Tenant.Id, User.Id,
            culture: new(UserCulture), calendar: settings.Calendar);
        CaseValueProvider = new CaseValueProvider(
            new CaseValueCache(settings.DbContext, GlobalCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
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
    /// Constructor for derived national cases
    /// </summary>
    protected DerivedCaseTool(
        IGlobalCaseValueRepository globalCaseValueRepository,
        INationalCaseValueRepository nationalCaseValueRepository,
        DerivedCaseToolSettings settings) :
        this(settings)
    {
        // repositories
        GlobalCaseValueRepository = globalCaseValueRepository ??
                                    throw new ArgumentNullException(nameof(globalCaseValueRepository));
        NationalCaseValueRepository = nationalCaseValueRepository ??
                                      throw new ArgumentNullException(nameof(nationalCaseValueRepository));
        // local
        var calculator = settings.PayrollCalculatorProvider.CreateCalculator(Tenant.Id, User.Id,
            culture: new(UserCulture), calendar: settings.Calendar);
        CaseValueProvider = new CaseValueProvider(
            new CaseValueCache(settings.DbContext, GlobalCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new CaseValueCache(settings.DbContext, NationalCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
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
    /// Constructor for derived company cases
    /// </summary>
    protected DerivedCaseTool(
        IGlobalCaseValueRepository globalCaseValueRepository,
        INationalCaseValueRepository nationalCaseValueRepository,
        ICompanyCaseValueRepository companyCaseValueRepository,
        DerivedCaseToolSettings settings) :
        this(settings)
    {
        // repositories
        GlobalCaseValueRepository = globalCaseValueRepository ??
                                    throw new ArgumentNullException(nameof(globalCaseValueRepository));
        NationalCaseValueRepository = nationalCaseValueRepository ??
                                      throw new ArgumentNullException(nameof(nationalCaseValueRepository));
        CompanyCaseValueRepository = companyCaseValueRepository ??
                                     throw new ArgumentNullException(nameof(companyCaseValueRepository));

        // local
        var calculator = settings.PayrollCalculatorProvider.CreateCalculator(Tenant.Id, User.Id,
            culture: new(UserCulture), calendar: settings.Calendar);
        CaseValueProvider = new CaseValueProvider(
            new CaseValueCache(settings.DbContext, GlobalCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new CaseValueCache(settings.DbContext, NationalCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new CaseValueCache(settings.DbContext, CompanyCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
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
    /// Constructor for derived employee cases
    /// </summary>
    protected DerivedCaseTool(Employee employee,
        IGlobalCaseValueRepository globalCaseValueRepository,
        INationalCaseValueRepository nationalCaseValueRepository,
        ICompanyCaseValueRepository companyCaseValueRepository,
        IEmployeeCaseValueRepository employeeCaseValueRepository,
        DerivedCaseToolSettings settings) :
        this(settings)
    {
        Employee = employee ?? throw new ArgumentNullException(nameof(employee));

        // repositories
        GlobalCaseValueRepository = globalCaseValueRepository ??
                                    throw new ArgumentNullException(nameof(globalCaseValueRepository));
        NationalCaseValueRepository = nationalCaseValueRepository ??
                                      throw new ArgumentNullException(nameof(nationalCaseValueRepository));
        CompanyCaseValueRepository = companyCaseValueRepository ??
                                     throw new ArgumentNullException(nameof(companyCaseValueRepository));
        EmployeeCaseValueRepository = employeeCaseValueRepository ??
                                      throw new ArgumentNullException(nameof(employeeCaseValueRepository));

        // local
        var calculator = settings.PayrollCalculatorProvider.CreateCalculator(Tenant.Id, User.Id,
            culture: new(UserCulture), calendar: settings.Calendar);
        CaseValueProvider = new CaseValueProvider(Employee,
            new CaseValueCache(settings.DbContext, GlobalCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new CaseValueCache(settings.DbContext, NationalCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new CaseValueCache(settings.DbContext, CompanyCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new CaseValueCache(settings.DbContext, EmployeeCaseValueRepository, Employee.Id, Payroll.DivisionId, EvaluationDate),
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
    private DerivedCaseTool(DerivedCaseToolSettings settings) :
        base(settings)
    {
        // global
        Tenant = settings.Tenant ?? throw new ArgumentNullException(nameof(settings.Tenant));
        TenantCulture = GetTenantCulture(Tenant);
        User = settings.User ?? throw new ArgumentNullException(nameof(settings.User));
        Payroll = settings.Payroll ?? throw new ArgumentNullException(nameof(settings.Payroll));

        // local
        if (!string.IsNullOrWhiteSpace(settings.ClusterSetName) && Payroll.ClusterSets != null)
        {
            ClusterSet = Payroll.ClusterSets.FirstOrDefault(x => string.Equals(settings.ClusterSetName, x.Name));
        }
        RegulationDate = settings.RegulationDate.IsUtc()
            ? settings.RegulationDate
            : throw new ArgumentException("Validation date must be UTC", nameof(settings.RegulationDate));
        EvaluationDate = settings.EvaluationDate.IsUtc()
            ? settings.EvaluationDate
            : throw new ArgumentException("Evaluation date must be UTC", nameof(settings.EvaluationDate));
        CaseProvider = new CaseProvider(Tenant, settings.PayrollRepository, RegulationDate, EvaluationDate);
        CaseFieldProvider = new CaseFieldProvider(
            new CaseFieldProxyRepository(settings.PayrollRepository, Tenant.Id, Payroll.Id, RegulationDate, EvaluationDate, ClusterSet));

        // repositories
        DivisionRepository = settings.DivisionRepository ?? throw new ArgumentNullException(nameof(settings.DivisionRepository));
        EmployeeRepository = settings.EmployeeRepository ?? throw new ArgumentNullException(nameof(settings.EmployeeRepository));
        PayrollRepository = settings.PayrollRepository ?? throw new ArgumentNullException(nameof(settings.PayrollRepository));
        RegulationRepository = settings.RegulationRepository ?? throw new ArgumentNullException(nameof(settings.RegulationRepository));
        RegulationLookupSetRepository = settings.LookupSetRepository ?? throw new ArgumentNullException(nameof(settings.LookupSetRepository));

        // services
        CalendarRepository = settings.CalendarRepository ?? throw new ArgumentNullException(nameof(settings.CalendarRepository));
        PayrollCalculatorProvider = settings.PayrollCalculatorProvider ?? throw new ArgumentNullException(nameof(settings.PayrollCalculatorProvider));
        WebhookDispatchService = settings.WebhookDispatchService ?? throw new ArgumentNullException(nameof(settings.WebhookDispatchService));
    }

    private static CultureInfo GetTenantCulture(Tenant tenant)
    {
        var culture = CultureInfo.DefaultThreadCurrentCulture ?? CultureInfo.InvariantCulture;
        if (!string.IsNullOrWhiteSpace(tenant.Culture) &&
            !string.Equals(culture.Name, tenant.Culture))
        {
            culture = new CultureInfo(tenant.Culture);
        }
        return culture;
    }

    protected async Task<IRegulationLookupProvider> NewRegulationLookupProviderAsync()
    {
        var lookups = (await PayrollRepository.GetDerivedLookupsAsync(Settings.DbContext,
            new()
            {
                TenantId = Tenant.Id,
                PayrollId = Payroll.Id,
                RegulationDate = RegulationDate,
                EvaluationDate = EvaluationDate
            },
            overrideType: OverrideType.Active)).ToList();
        return new RegulationLookupProvider(lookups, RegulationRepository, RegulationLookupSetRepository);
    }

    /// <summary>
    /// Gets the derived case set
    /// </summary>
    /// <param name="cases">The cases</param>
    /// <param name="caseSlot">The case slot</param>
    /// <param name="caseChangeSetup">The case change setup</param>
    /// <param name="culture">The culture</param>
    /// <param name="initValues">if set to <c>true</c> [initialize values].</param>
    /// <returns>The derived case set</returns>
    protected async Task<CaseSet> GetDerivedCaseSetAsync(IList<Case> cases, string caseSlot,
        CaseChangeSetup caseChangeSetup, string culture, bool initValues)
    {
        if (cases == null)
        {
            throw new ArgumentNullException(nameof(cases));
        }

        // build derived case
        var derivedCase = DerivedCaseFactory.BuildCase(cases, caseSlot, culture);

        //  case fields
        var allCaseFields = await GetDerivedCaseFieldsAsync(derivedCase);
        if (allCaseFields.Any())
        {
            derivedCase.Fields = [];

            // group case fields by the case field name
            var caseFieldsByName = allCaseFields.GroupBy(x => x.Name, y => y);
            foreach (var caseFields in caseFieldsByName)
            {
                // build derived case field
                var derivedCaseField = DerivedCaseFactory.BuildCaseField(caseFields, culture);
                derivedCase.Fields.Add(derivedCaseField);
            }

            // case values
            var caseCulture = string.IsNullOrWhiteSpace(culture) ?
                                CultureInfo.CurrentCulture : new CultureInfo(culture);
            foreach (var caseField in derivedCase.Fields)
            {
                // adapt value
                var caseValue = caseChangeSetup.FindCaseValue(caseField.Name, caseSlot);
                if (caseValue != null)
                {
                    caseField.Start = caseValue.Start;
                    caseField.End = caseValue.End;
                    caseField.Value = caseValue.Value;
                    caseField.CancellationDate = caseValue.CancellationDate;
                }
                else if (initValues)
                {
                    // init case field without time value
                    if (caseField.TimeType is CaseFieldTimeType.Timeless or CaseFieldTimeType.Moment)
                    {
                        if (caseField.DefaultStart != null)
                        {
                            caseField.Start = Date.Parse(caseField.DefaultStart, caseCulture);
                        }
                        if (caseField.DefaultEnd != null)
                        {
                            caseField.End = Date.Parse(caseField.DefaultEnd, caseCulture);
                        }
                        if (caseField.DefaultValue != null)
                        {
                            caseField.Value = caseField.ValueType is ValueType.Date or ValueType.DateTime
                                ? ValueConvert.ToJson(Date.Parse(caseField.DefaultValue, caseCulture))
                                : caseField.DefaultValue;
                        }
                    }
                    else
                    {
                        // init time value
                        var caseValueReference = new CaseValueReference(caseField.Name, caseSlot);
                        var currentCaseValue = (await CaseValueProvider.GetTimeCaseValuesAsync(
                            valueDate: EvaluationDate,
                            caseType: derivedCase.CaseType,
                            caseFieldNames: [caseValueReference.Reference])).FirstOrDefault();

                        // default or previous values
                        caseField.Start = currentCaseValue?.Start ??
                                          (caseField.DefaultStart != null ?
                                            Date.Parse(caseField.DefaultStart, caseCulture) : null);
                        caseField.End = currentCaseValue?.End ??
                                        (caseField.DefaultEnd != null ?
                                            Date.Parse(caseField.DefaultEnd, caseCulture) : null);
                        if (currentCaseValue?.Value == null)
                        {
                            caseField.Value = caseField.ValueType is ValueType.Date or ValueType.DateTime ?
                                ValueConvert.ToJson(Date.Parse(caseField.DefaultValue, caseCulture)) :
                                caseField.DefaultValue;
                        }
                        else
                        {
                            caseField.Value = currentCaseValue.Value;
                        }
                        caseField.CancellationDate = currentCaseValue?.CancellationDate;
                    }
                }
            }
        }

        return derivedCase;
    }

    private async Task<List<ChildCaseField>> GetDerivedCaseFieldsAsync(CaseSet derivedCase) =>
        await ResolveCaseFieldsAsync(derivedCase);

    // recursive entry point
    private async Task<List<ChildCaseField>> ResolveCaseFieldsAsync(Case @case)
    {
        var resolvedCaseFields = new List<ChildCaseField>();

        // collect fields of base case
        if (!string.IsNullOrWhiteSpace(@case.BaseCase))
        {
            var baseCase = (await PayrollRepository.GetDerivedCasesAsync(Settings.DbContext,
                new()
                {
                    TenantId = Tenant.Id,
                    PayrollId = Payroll.Id,
                    RegulationDate = RegulationDate,
                    EvaluationDate = EvaluationDate
                },
                caseNames: [@case.BaseCase],
                clusterSet: ClusterSet,
                overrideType: OverrideType.Active)).FirstOrDefault();
            if (baseCase == null)
            {
                throw new PayrollException($"Unknown base case {@case.BaseCase} in case {@case.Name}.");
            }

            // base collect case fields of base case (recursive)
            AddCaseFields(await ResolveCaseFieldsAsync(baseCase), resolvedCaseFields);
        }

        // collect base case fields
        if (@case.BaseCaseFields != null && @case.BaseCaseFields.Any())
        {
            var caseFieldNames = new HashSet<string>(@case.BaseCaseFields.Select(x => x.Name));
            if (caseFieldNames.Any())
            {
                var baseCaseFields =
                    (await PayrollRepository.GetDerivedCaseFieldsAsync(Settings.DbContext,
                        new()
                        {
                            TenantId = Tenant.Id,
                            PayrollId = Payroll.Id,
                            RegulationDate = RegulationDate,
                            EvaluationDate = EvaluationDate
                        },
                        caseFieldNames: caseFieldNames,
                        clusterSet: ClusterSet,
                        overrideType: OverrideType.Active)).ToList();

                // add base case fields, ensure unique case field names
                if (baseCaseFields.Any())
                {
                    // case fields by name
                    var baseCaseFieldByNames = baseCaseFields.GroupBy(x => x.Name, y => y);
                    foreach (var baseCaseFieldByName in baseCaseFieldByNames)
                    {
                        var caseField = baseCaseFieldByName.First();
                        var caseFieldReference = @case.BaseCaseFields.First(x => string.Equals(x.Name, caseField.Name));
                        if (caseFieldReference.Order.HasValue)
                        {
                            caseField.Order = caseFieldReference.Order.Value;
                        }
                        // add case field
                        AddCaseFields([caseField], resolvedCaseFields);
                    }
                }
            }
        }

        // collect current case fields
        var caseFields =
            (await PayrollRepository.GetDerivedCaseFieldsOfCaseAsync(Settings.DbContext,
                new()
                {
                    TenantId = Tenant.Id,
                    PayrollId = Payroll.Id,
                    RegulationDate = RegulationDate,
                    EvaluationDate = EvaluationDate
                },
                caseNames: [@case.Name],
                clusterSet: ClusterSet,
                overrideType: OverrideType.Active)).ToList();

        // add case fields, ensure unique case field names
        if (caseFields.Any())
        {
            // iterate from last to first and add the base case fields before the extension case fields
            AddCaseFields(caseFields, resolvedCaseFields);
        }

        // sort case fields by order and then by id
        return resolvedCaseFields.OrderBy(x => x.Order).ThenBy(x => x.Id).ToList();
    }

    private static void AddCaseFields(List<ChildCaseField> sourceCaseFields, List<ChildCaseField> targetCaseFields)
    {
        foreach (var caseField in sourceCaseFields)
        {
            if (targetCaseFields.Any(x => string.Equals(x.Name, caseField.Name)))
            {
                // ignore base case fields
                continue;
            }
            targetCaseFields.Add(caseField);
        }
    }
}
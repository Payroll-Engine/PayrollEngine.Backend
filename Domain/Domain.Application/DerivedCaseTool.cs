using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting;

namespace PayrollEngine.Domain.Application;

public abstract class DerivedCaseTool : FunctionToolBase
{
    // settings
    protected new DerivedCaseToolSettings Settings => base.Settings as DerivedCaseToolSettings;

    // providers
    protected ICaseProvider CaseProvider { get; }
    protected ICaseFieldProvider CaseFieldProvider { get; }
    protected ICaseValueProvider CaseValueProvider { get; }

    /// <summary>
    /// The webhook dispatcher
    /// </summary>
    public IWebhookDispatchService WebhookDispatchService { get; }

    /// <summary>
    /// The regulation date
    /// </summary>
    public DateTime RegulationDate { get; }

    /// <summary>
    /// The evaluation date
    /// </summary>
    public DateTime EvaluationDate { get; }

    /// <summary>
    /// The cluster set
    /// </summary>
    protected ClusterSet ClusterSet { get; }

    // global
    public CultureInfo Culture { get; }
    public Tenant Tenant { get; }
    public User User { get; }
    public Payroll Payroll { get; }
    public Employee Employee { get; }

    // repositories
    public IPayrollRepository PayrollRepository { get; }
    public ICaseRepository CaseRepository { get; }
    public ICaseRelationRepository CaseRelationRepository { get; }
    public IRegulationRepository RegulationRepository { get; }
    public ILookupSetRepository RegulationLookupSetRepository { get; }

    public IGlobalCaseValueRepository GlobalCaseValueRepository { get; }
    public INationalCaseValueRepository NationalCaseValueRepository { get; }
    public ICompanyCaseValueRepository CompanyCaseValueRepository { get; }
    public IEmployeeCaseValueRepository EmployeeCaseValueRepository { get; }

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
        var calculator = settings.PayrollCalculatorProvider.CreateCalculator(Payroll.CalendarCalculationMode, Tenant.Id, User.Id);
        CaseValueProvider = new CaseValueProvider(
            new CaseValueCache(settings.DbContext, GlobalCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new()
            {
                DbContext = Settings.DbContext,
                FunctionHost = FunctionHost,
                Tenant = Tenant,
                CaseRepository = CaseRepository,
                Calculator = calculator,
                CaseFieldProvider = CaseFieldProvider,
                RegulationLookupProvider = settings.RegulationLookupProvider,
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
        var calculator = settings.PayrollCalculatorProvider.CreateCalculator(Payroll.CalendarCalculationMode, Tenant.Id, User.Id);
        CaseValueProvider = new CaseValueProvider(
            new CaseValueCache(settings.DbContext, GlobalCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new CaseValueCache(settings.DbContext, NationalCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new()
            {
                DbContext = Settings.DbContext,
                FunctionHost = FunctionHost,
                Tenant = Tenant,
                CaseRepository = CaseRepository,
                Calculator = calculator,
                CaseFieldProvider = CaseFieldProvider,
                RegulationLookupProvider = settings.RegulationLookupProvider,
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
        var calculator = settings.PayrollCalculatorProvider.CreateCalculator(Payroll.CalendarCalculationMode, Tenant.Id, User.Id);
        CaseValueProvider = new CaseValueProvider(
            new CaseValueCache(settings.DbContext, GlobalCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new CaseValueCache(settings.DbContext, NationalCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new CaseValueCache(settings.DbContext, CompanyCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new()
            {
                DbContext = Settings.DbContext,
                FunctionHost = FunctionHost,
                Tenant = Tenant,
                CaseRepository = CaseRepository,
                Calculator = calculator,
                CaseFieldProvider = CaseFieldProvider,
                RegulationLookupProvider = settings.RegulationLookupProvider,
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
        var calculator = settings.PayrollCalculatorProvider.CreateCalculator(Payroll.CalendarCalculationMode, Tenant.Id, User.Id);
        CaseValueProvider = new CaseValueProvider(Employee,
            new CaseValueCache(settings.DbContext, GlobalCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new CaseValueCache(settings.DbContext, NationalCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new CaseValueCache(settings.DbContext, CompanyCaseValueRepository, Tenant.Id, Payroll.DivisionId, EvaluationDate),
            new CaseValueCache(settings.DbContext, EmployeeCaseValueRepository, Employee.Id, Payroll.DivisionId, EvaluationDate),
            new()
            {
                DbContext = Settings.DbContext,
                FunctionHost = FunctionHost,
                Tenant = Tenant,
                CaseRepository = CaseRepository,
                Calculator = calculator,
                CaseFieldProvider = CaseFieldProvider,
                RegulationLookupProvider = settings.RegulationLookupProvider,
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
        Culture = settings.Culture ?? CultureInfo.CurrentCulture;
        Tenant = settings.Tenant ?? throw new ArgumentNullException(nameof(settings.Tenant));
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
        PayrollRepository = settings.PayrollRepository ?? throw new ArgumentNullException(nameof(settings.PayrollRepository));
        CaseRepository = settings.CaseRepository ?? throw new ArgumentNullException(nameof(settings.CaseRepository));
        CaseRelationRepository = settings.CaseRelationRepository ?? throw new ArgumentNullException(nameof(settings.CaseRelationRepository));
        RegulationRepository = settings.RegulationRepository ?? throw new ArgumentNullException(nameof(settings.RegulationRepository));
        RegulationLookupSetRepository = settings.LookupSetRepository ?? throw new ArgumentNullException(nameof(settings.LookupSetRepository));

        // services
        WebhookDispatchService = settings.WebhookDispatchService ?? throw new ArgumentNullException(nameof(settings.WebhookDispatchService));
    }

    protected virtual async Task<IRegulationLookupProvider> NewRegulationLookupProviderAsync()
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
    /// <param name="language">The language</param>
    /// <param name="initValues">if set to <c>true</c> [initialize values].</param>
    /// <returns>The derived case set</returns>
    protected virtual async Task<CaseSet> GetDerivedCaseSetAsync(IList<Case> cases, string caseSlot,
        CaseChangeSetup caseChangeSetup, Language language, bool initValues)
    {
        if (cases == null)
        {
            throw new ArgumentNullException(nameof(cases));
        }

        // build derived case
        var derivedCase = DerivedCaseFactory.BuildCase(cases, caseSlot, language);

        //  case fields
        var allCaseFields = await GetDerivedCaseFieldsAsync(derivedCase);
        if (allCaseFields.Any())
        {
            derivedCase.Fields = new();

            // group case fields by the case field name
            var caseFieldsByName = allCaseFields.GroupBy(x => x.Name, y => y);
            foreach (var caseFields in caseFieldsByName)
            {
                // build derived case field
                var derivedCaseField = DerivedCaseFactory.BuildCaseField(caseFields, language);
                derivedCase.Fields.Add(derivedCaseField);
            }

            // case values
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
                    // no default values: get case value at the evaluation date
                    if (caseField.DefaultStart == null && caseField.DefaultEnd == null &&
                        caseField.DefaultValue == null)
                    {
                        var caseValueReference = new CaseValueReference(caseField.Name, caseSlot);
                        var currentCaseValue = (await CaseValueProvider.GetTimeCaseValuesAsync(
                            EvaluationDate, derivedCase.CaseType, new[] { caseValueReference.Reference })).FirstOrDefault();
                        if (currentCaseValue != null)
                        {
                            caseField.Start = currentCaseValue.Start;
                            caseField.End = currentCaseValue.End;
                            caseField.Value = currentCaseValue.Value;
                            caseField.CancellationDate = currentCaseValue.CancellationDate;
                        }
                    }
                    // default value
                    else if (!string.IsNullOrWhiteSpace(caseField.DefaultValue))
                    {
                        caseField.Value = caseField.DefaultValue;
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
                caseNames: new[] { @case.BaseCase },
                clusterSet: ClusterSet,
                overrideType: OverrideType.Active)).FirstOrDefault();
            if (baseCase == null)
            {
                throw new PayrollException($"Unknown base case {@case.BaseCase} in case {@case.Name}");
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
                        AddCaseFields(new() { caseField }, resolvedCaseFields);
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
                caseNames: new[] { @case.Name },
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
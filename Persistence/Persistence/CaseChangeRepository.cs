using PayrollEngine.Domain.Model;
using System;
using System.Data;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public abstract class CaseChangeRepository<T>(string tableName, string parentFieldName,
        CaseChangeRepositorySettings settings)
    : ChildDomainRepository<T>(tableName, parentFieldName), ICaseChangeRepository<T>
    where T : CaseChange
{
    private ITenantRepository TenantRepository { get; } = settings.TenantRepository ?? throw new ArgumentNullException(nameof(CaseChangeRepositorySettings.TenantRepository));
    private IDivisionRepository DivisionRepository { get; } = settings.DivisionRepository ?? throw new ArgumentNullException(nameof(CaseChangeRepositorySettings.DivisionRepository));
    private IEmployeeRepository EmployeeRepository { get; } = settings.EmployeeRepository ?? throw new ArgumentNullException(nameof(CaseChangeRepositorySettings.EmployeeRepository));
    private IPayrollRepository PayrollRepository { get; } = settings.PayrollRepository ?? throw new ArgumentNullException(nameof(CaseChangeRepositorySettings.PayrollRepository));
    private ICaseRepository CaseRepository { get; } = settings.CaseRepository ?? throw new ArgumentNullException(nameof(CaseChangeRepositorySettings.CaseRepository));
    private ICaseFieldRepository CaseFieldRepository { get; } = settings.CaseFieldRepository ?? throw new ArgumentNullException(nameof(CaseChangeRepositorySettings.CaseFieldRepository));
    private ICaseValueRepository CaseValueRepository { get; } = settings.CaseValueRepository ?? throw new ArgumentNullException(nameof(CaseChangeRepositorySettings.CaseValueRepository));
    private ICaseValueSetupRepository CaseValueSetupRepository { get; } = settings.CaseValueSetupRepository ?? throw new ArgumentNullException(nameof(CaseChangeRepositorySettings.CaseValueSetupRepository));
    private ICaseValueChangeRepository CaseValueChangeRepository { get; } = settings.CaseValueChangeRepository ?? throw new ArgumentNullException(nameof(CaseChangeRepositorySettings.CaseValueChangeRepository));

    protected override void GetObjectCreateData(T caseChange, DbParameterCollection parameters)
    {
        parameters.Add(nameof(caseChange.UserId), caseChange.UserId, DbType.Int32);
        parameters.Add(nameof(caseChange.DivisionId), caseChange.DivisionId, DbType.Int32);
        parameters.Add(nameof(caseChange.Reason), caseChange.Reason);
        parameters.Add(nameof(caseChange.CancellationType), caseChange.CancellationType, DbType.Int32);
        parameters.Add(nameof(caseChange.ValidationCaseName), caseChange.ValidationCaseName);
        parameters.Add(nameof(caseChange.Forecast), caseChange.Forecast);
        base.GetObjectCreateData(caseChange, parameters);
    }

    protected override void GetObjectData(T caseChange, DbParameterCollection parameters)
    {
        parameters.Add(nameof(caseChange.CancellationId), caseChange.CancellationId, DbType.Int32);
        parameters.Add(nameof(caseChange.CancellationDate), caseChange.CancellationDate, DbType.DateTime2);
        base.GetObjectData(caseChange, parameters);
    }

    #region Query

    protected abstract Task<IEnumerable<CaseChangeCaseValue>> QueryCaseChangesValuesAsync(IDbContext context, int tenantId, int parentId, Query query = null);
    protected abstract Task<long> QueryCaseChangesValuesCountAsync(IDbContext context, int tenantId, int parentId, Query query = null);

    public virtual async Task<IEnumerable<T>> QueryAsync(IDbContext context, int tenantId, int parentId, Query query = null)
    {
        // query
        var dbQuery = DbQueryFactory.NewQuery<T>(context, TableName, ParentFieldName, parentId, query);

        // case change query filter
        if (query is CaseChangeQuery caseChangeQuery)
        {
            caseChangeQuery.ApplyTo(dbQuery);
        }

        // query compilation
        var compileQuery = CompileQuery(dbQuery);

        // retrieve case changes
        var caseChanges = (await QueryAsync<T>(context, compileQuery)).ToList();

        // notification
        await OnRetrieved(context, parentId, caseChanges);

        if (caseChanges.Any())
        {
            // retrieve case values
            var caseValues = (await QueryCaseChangesValuesAsync(context, tenantId, parentId)).ToList();
            if (!caseValues.Any())
            {
                throw new PayrollException($"Missing case values for case change with parent id {parentId}.");
            }

            // apply case values
            foreach (var caseValue in caseValues)
            {
                var caseChange = caseChanges.FirstOrDefault(x => x.Id == caseValue.CaseChangeId);
                if (caseChange == null)
                {
                    // case change from another division
                    continue;
                }
                caseChange.Values ??= [];
                caseChange.Values.Add(caseValue);
            }
        }
        return caseChanges;
    }

    public virtual async Task<long> QueryCountAsync(IDbContext context, int tenantId, int parentId, Query query = null)
    {
        // query
        var dbQuery = DbQueryFactory.NewQuery<T>(context, TableName, ParentFieldName, parentId, query);

        // case change query filter
        if (query is CaseChangeQuery caseChangeQuery)
        {
            caseChangeQuery.ApplyTo(dbQuery);
        }

        // query compilation
        var compileQuery = CompileQuery(dbQuery);

        // SELECT execution
        var count = await QuerySingleAsync<long>(context, compileQuery);
        return count;
    }

    public virtual async Task<IEnumerable<CaseChangeCaseValue>> QueryValuesAsync(IDbContext context,
        int tenantId, int parentId, Query query = null)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentException(nameof(tenantId));
        }
        if (parentId <= 0)
        {
            throw new ArgumentException(nameof(parentId));
        }
        return (await QueryCaseChangesValuesAsync(context, tenantId, parentId, query)).ToList();
    }

    public virtual async Task<long> QueryValuesCountAsync(IDbContext context, int tenantId,
        int parentId, Query query = null)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentException(nameof(tenantId));
        }
        if (parentId <= 0)
        {
            throw new ArgumentException(nameof(parentId));
        }
        return await QueryCaseChangesValuesCountAsync(context, tenantId, parentId, query);
    }

    #endregion

    #region Add New Case

    public virtual async Task<T> AddCaseChangeAsync(IDbContext context, int tenantId, int payrollId, int parentId, T caseChange)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentException(nameof(tenantId));
        }
        if (payrollId <= 0)
        {
            throw new ArgumentException(nameof(payrollId));
        }
        if (parentId <= 0)
        {
            throw new ArgumentException(nameof(parentId));
        }
        if (caseChange == null)
        {
            throw new ArgumentNullException(nameof(caseChange));
        }
        if (caseChange.Values == null || !caseChange.Values.Any())
        {
            throw new PayrollException("Case change without values.");
        }

        // culture
        var culture = await GetChangeCultureAsync(context, tenantId, caseChange);

        CaseType? caseType = null;

        // sync value created date
        if (caseChange.Created != DateTime.MinValue)
        {
            foreach (var caseValue in caseChange.Values)
            {
                if (caseValue.Created == DateTime.MinValue)
                {
                    caseValue.Created = caseChange.Created;
                }
            }
        }

        // collect cases with case fields
        var cases = new List<Case>();
        var caseFields = new List<CaseField>();
        foreach (var caseValue in caseChange.Values)
        {
            // case field
            var caseField = (await PayrollRepository.GetDerivedCaseFieldsAsync(context,
                new() { TenantId = tenantId, PayrollId = payrollId },
                [caseValue.CaseFieldName])).FirstOrDefault();
            if (caseField == null)
            {
                throw new PayrollException($"Unknown case field {caseValue.CaseFieldName}.");
            }
            caseFields.Add(caseField);

            // case
            Case @case;
            if (string.IsNullOrWhiteSpace(caseChange.ValidationCaseName))
            {
                var caseId = await CaseFieldRepository.GetParentIdAsync(context, caseField.Id);
                if (!caseId.HasValue)
                {
                    throw new PayrollException($"Unknown case for case field {caseField}.");
                }
                var regulationId = await CaseRepository.GetParentIdAsync(context, caseId.Value);
                if (!regulationId.HasValue)
                {
                    throw new PayrollException($"Unknown regulation case with id {caseId} on case field {caseField}.");
                }

                @case = await CaseRepository.GetAsync(context, regulationId.Value, caseId.Value);
            }
            else
            {
                @case = (await PayrollRepository.GetDerivedCasesAsync(context,
                            query: new() { TenantId = tenantId, PayrollId = payrollId },
                            caseNames: [caseChange.ValidationCaseName])).FirstOrDefault();
            }
            if (@case == null)
            {
                throw new PayrollException($"Missing case for case field {caseField}.");
            }
            cases.Add(@case);
        }

        // case change division
        if (caseChange.DivisionId.HasValue && caseFields.All(x => x.ValueScope != ValueScope.Local))
        {
            // reset division on change with global values
            caseChange.DivisionId = null;
        }

        // remove unchanged case value
        var updateFields = new List<CaseField>();
        var updateValues = new List<CaseValue>();
        for (var i = 0; i < caseChange.Values.Count; i++)
        {
            var caseField = caseFields[i];
            var caseValue = caseChange.Values[i];

            var createValue = true;
            switch (caseField.ValueCreationMode)
            {
                case CaseValueCreationMode.OnChanges:

                    var filter = $"{nameof(CaseValue.CaseFieldName)} eq '{caseField.Name}'";
                    if (caseChange.DivisionId.HasValue)
                    {
                        filter += $" and {nameof(CaseValue.DivisionId)} eq {caseChange.DivisionId.Value}";
                    }

                    // query for the last created case value
                    var query = new Query
                    {
                        Top = 1,
                        OrderBy = $"{nameof(CaseValue.Created)} DESC",
                        Filter = filter
                    };
                    var latestCaseValue = (await CaseValueRepository.QueryAsync(context, parentId, query)).FirstOrDefault();
                    if (latestCaseValue != null && EqualCaseValue(latestCaseValue, caseValue) ||
                        (latestCaseValue == null && caseValue?.Value == null))
                    {
                        // unchanged or undefined value
                        createValue = false;
                    }
                    break;
                case CaseValueCreationMode.Always:
                    break;
                case CaseValueCreationMode.Discard:
                    createValue = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (createValue)
            {
                // change case field
                updateFields.Add(caseField);
                // changed case value
                updateValues.Add(caseValue);
            }
            else
            {
                // ignored case value
                caseChange.IgnoredValues ??= [];
                caseChange.IgnoredValues.Add(caseValue);
            }
        }
        // apply update fields and values
        caseFields = updateFields;
        caseChange.Values = updateValues;

        // case change without any changed case value
        if (!updateFields.Any())
        {
            return caseChange;
        }

        // transaction
        using var txScope = TransactionFactory.NewTransactionScope();

        // create case change
        await CreateAsync(context, parentId, caseChange);

        // create case values
        for (var i = 0; i < caseChange.Values.Count; i++)
        {
            var caseValue = caseChange.Values[i];
            var @case = cases[i];
            var caseField = caseFields[i];

            // case slot
            if (!string.IsNullOrWhiteSpace(caseValue.CaseSlot) && caseValue.CaseSlotLocalizations == null)
            {
                // case slot localization from case
                var slot = @case.Slots?.FirstOrDefault(x => string.Equals(x.Name, caseValue.CaseSlot));
                if (slot != null)
                {
                    caseValue.CaseSlotLocalizations = slot.NameLocalizations;
                }
            }

            // case name
            caseValue.CaseName = @case.Name;
            caseValue.CaseNameLocalizations = @case.NameLocalizations;

            // culture
            caseField.Culture =
                // culture priority 1: case value
                caseValue.Culture ??
                // culture priority 2 to 5: employee > division > tenant > system
                culture;

            // case field name
            caseValue.CaseFieldNameLocalizations = caseField.NameLocalizations;

            // ensure change forecast on all values
            caseValue.Forecast = caseChange.Forecast;

            // case value tags
            caseValue.Tags = caseValue.Tags;

            // case type
            if (caseType.HasValue)
            {
                if (@case.CaseType != caseType.Value)
                {
                    throw new PayrollException($"Different case types within a case change: {@case.CaseType} and {caseType.Value}.");
                }
            }
            else
            {
                caseType = @case.CaseType;
            }

            // division: global or local value
            switch (caseField.ValueScope)
            {
                case ValueScope.Local:
                    var changeDivision = caseChange.DivisionId.HasValue;
                    var valueDivision = caseValue.DivisionId.HasValue;

                    // different divisions between case change and case value
                    if (changeDivision && valueDivision && caseChange.DivisionId.Value != caseValue.DivisionId.Value)
                    {
                        throw new PayrollException($"Invalid case value division on case field: {caseField.Name}.");
                    }

                    // local case value requires a division
                    if (!changeDivision && !valueDivision)
                    {
                        throw new PayrollException($"Missing case value division on case field: {caseField.Name}.");
                    }

                    // ensure division from case change
                    if (!valueDivision)
                    {
                        caseValue.DivisionId = caseChange.DivisionId;
                    }
                    break;
                case ValueScope.Global:
                    // ensure no division on global case value
                    caseValue.DivisionId = null;
                    break;
            }

            // value type and kind
            caseValue.ValueType = caseField.ValueType;

            // ensure json string
            caseValue.Value ??= string.Empty;

            // culture
            // [culture by priority]: case-field > employee > division > tenant
            caseValue.Culture = caseField.Culture ?? culture;

            // case value setup
            if (caseValue is CaseValueSetup caseValueSetup)
            {
                // with documents
                await CaseValueSetupRepository.CreateAsync(context, parentId, caseValueSetup);
            }
            else
            {
                // without documents
                await CaseValueRepository.CreateAsync(context, parentId, caseValue);
            }
        }

        // create case value changes
        await CreateCaseValuesAsync(context, caseChange);

        // cancellation case change: update cancellation data on cancelled case change
        if (caseChange.CancellationId.HasValue)
        {
            var cancellationCaseChange = await GetAsync(context, parentId, caseChange.CancellationId.Value);
            if (cancellationCaseChange == null)
            {
                throw new PayrollException($"Invalid cancellation case change with id {caseChange.CancellationId.Value}.");
            }

            // prevent double cancellations
            if (cancellationCaseChange.CancellationId.HasValue)
            {
                throw new PayrollException($"Case change with id {cancellationCaseChange.Id} already cancelled.");
            }

            // update cancellation state
            cancellationCaseChange.CancellationId = caseChange.Id;
            cancellationCaseChange.CancellationDate = caseChange.CancellationDate;
            await UpdateAsync(context, parentId, cancellationCaseChange);
        }

        txScope.Complete();
        return caseChange;
    }

    private async Task<string> GetChangeCultureAsync(IDbContext context, int tenantId, T caseChange)
    {
        // priority 1: employee culture
        if (caseChange.EmployeeId.HasValue)
        {
            var employee = await EmployeeRepository.GetAsync(context, tenantId, caseChange.EmployeeId.Value);
            if (employee == null)
            {
                throw new PayrollException($"Unknown case change employee with id {caseChange.EmployeeId}.");
            }
            if (!string.IsNullOrWhiteSpace(employee.Culture))
            {
                return employee.Culture;
            }
        }

        // priority 2: division culture
        if (caseChange.DivisionId.HasValue)
        {
            var division = await DivisionRepository.GetAsync(context, tenantId, caseChange.DivisionId.Value);
            if (division == null)
            {
                throw new PayrollException($"Unknown case change division with id {caseChange.DivisionId}.");
            }
            if (!string.IsNullOrWhiteSpace(division.Culture))
            {
                return division.Culture;
            }
        }

        // priority 3: tenant culture
        var tenant = await TenantRepository.GetAsync(context, tenantId);
        if (tenant == null)
        {
            throw new PayrollException($"Unknown case change tenant with id {tenantId}.");
        }
        if (!string.IsNullOrWhiteSpace(tenant.Culture))
        {
            return tenant.Culture;
        }

        // priority 4: system
        return CultureInfo.CurrentCulture.Name;
    }

    private static bool EqualCaseValue(CaseValue left, CaseValue right)
    {
        // start and end
        if (!Equals(left.Start, right.Start) || !Equals(left.End, right.End))
        {
            return false;
        }

        // value
        return Equals(left.Value, right.Value) ||
               (string.IsNullOrWhiteSpace(left.Value) &&
                string.IsNullOrWhiteSpace(right.Value));
    }

    private async Task CreateCaseValuesAsync(IDbContext context, T caseChange)
    {
        foreach (var caseValue in caseChange.Values)
        {
            var valueChange = new CaseValueChange
            {
                CaseChangeId = caseChange.Id,
                CaseValueId = caseValue.Id
            };
            await CaseValueChangeRepository.CreateAsync(context, caseChange.Id, valueChange);
        }
    }

    #endregion

}
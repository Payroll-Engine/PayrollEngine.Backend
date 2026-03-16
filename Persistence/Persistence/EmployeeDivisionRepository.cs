using System;
using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

/// <summary>Repository for <see cref="EmployeeDivision"/> persistence (table: EmployeeDivision).</summary>
public class EmployeeDivisionRepository(IDivisionRepository divisionRepository) :
    ChildDomainRepository<EmployeeDivision>(Tables.EmployeeDivision,
        EmployeeDivisionColumn.EmployeeId), IEmployeeDivisionRepository
{
    public IDivisionRepository DivisionRepository { get; } = divisionRepository ?? throw new ArgumentNullException(nameof(divisionRepository));

    protected override void GetObjectData(EmployeeDivision division, DbParameterCollection parameters)
    {
        parameters.Add(nameof(division.EmployeeId), division.EmployeeId, DbType.Int32);
        parameters.Add(nameof(division.DivisionId), division.DivisionId, DbType.Int32);
        base.GetObjectData(division, parameters);
    }
}
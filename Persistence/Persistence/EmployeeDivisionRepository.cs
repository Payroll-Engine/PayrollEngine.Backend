using System;
using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class EmployeeDivisionRepository(IDivisionRepository divisionRepository) :
    ChildDomainRepository<EmployeeDivision>(DbSchema.Tables.EmployeeDivision,
        DbSchema.EmployeeDivisionColumn.EmployeeId), IEmployeeDivisionRepository
{
    public IDivisionRepository DivisionRepository { get; } = divisionRepository ?? throw new ArgumentNullException(nameof(divisionRepository));

    protected override void GetObjectData(EmployeeDivision division, DbParameterCollection parameters)
    {
        parameters.Add(nameof(division.EmployeeId), division.EmployeeId, DbType.Int32);
        parameters.Add(nameof(division.DivisionId), division.DivisionId, DbType.Int32);
        base.GetObjectData(division, parameters);
    }
}
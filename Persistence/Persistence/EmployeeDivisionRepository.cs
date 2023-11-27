using System;
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
        parameters.Add(nameof(division.EmployeeId), division.EmployeeId);
        parameters.Add(nameof(division.DivisionId), division.DivisionId);
        base.GetObjectData(division, parameters);
    }
}
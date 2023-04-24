using System;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class EmployeeDivisionRepository : ChildDomainRepository<EmployeeDivision>, IEmployeeDivisionRepository
{
    public IDivisionRepository DivisionRepository { get; }

    public EmployeeDivisionRepository(IDivisionRepository divisionRepository) :
        base(DbSchema.Tables.EmployeeDivision, DbSchema.EmployeeDivisionColumn.EmployeeId)
    {
        DivisionRepository = divisionRepository ?? throw new ArgumentNullException(nameof(divisionRepository));
    }

    protected override void GetObjectData(EmployeeDivision division, DbParameterCollection parameters)
    {
        parameters.Add(nameof(division.EmployeeId), division.EmployeeId);
        parameters.Add(nameof(division.DivisionId), division.DivisionId);
        base.GetObjectData(division, parameters);
    }
}
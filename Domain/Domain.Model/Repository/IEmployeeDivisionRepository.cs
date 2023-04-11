namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for employee divisions
/// </summary>
public interface IEmployeeDivisionRepository : IChildDomainRepository<EmployeeDivision>
{
    /// <summary>
    /// The division repository
    /// </summary>
    public IDivisionRepository DivisionRepository { get; }
}
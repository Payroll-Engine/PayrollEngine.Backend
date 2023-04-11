namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for global case value changes
/// </summary>
public interface IGlobalCaseValueChangeRepository : ICaseValueChangeRepository
{
}

/// <summary>
/// Repository for national case value changes
/// </summary>
public interface INationalCaseValueChangeRepository : ICaseValueChangeRepository
{
}

/// <summary>
/// Repository for company case value changes
/// </summary>
public interface ICompanyCaseValueChangeRepository : ICaseValueChangeRepository
{
}

/// <summary>
/// Repository for employee case value changes
/// </summary>
public interface IEmployeeCaseValueChangeRepository : ICaseValueChangeRepository
{
}

/// <summary>
/// Repository for case value changes
/// </summary>
public interface ICaseValueChangeRepository : IChildDomainRepository<CaseValueChange>
{
}

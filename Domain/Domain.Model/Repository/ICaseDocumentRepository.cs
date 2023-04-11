namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for global case documents
/// </summary>
public interface IGlobalCaseDocumentRepository : ICaseDocumentRepository
{
}

/// <summary>
/// Repository for national case documents
/// </summary>
public interface INationalCaseDocumentRepository : ICaseDocumentRepository
{
}

/// <summary>
/// Repository for company case documents
/// </summary>
public interface ICompanyCaseDocumentRepository : ICaseDocumentRepository
{
}

/// <summary>
/// Repository for employee case documents
/// </summary>
public interface IEmployeeCaseDocumentRepository : ICaseDocumentRepository
{
}

/// <summary>
/// Repository for case documents
/// </summary>
public interface ICaseDocumentRepository : IChildDomainRepository<CaseDocument>
{
}
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class EmployeeCaseDocumentRepository() : CaseDocumentRepository(DbSchema.Tables.EmployeeCaseDocument,
    DbSchema.EmployeeCaseDocumentColumn.CaseValueId), IEmployeeCaseDocumentRepository;
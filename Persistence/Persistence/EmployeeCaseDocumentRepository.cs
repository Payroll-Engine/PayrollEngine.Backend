using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

public class EmployeeCaseDocumentRepository() : CaseDocumentRepository(Tables.EmployeeCaseDocument,
    EmployeeCaseDocumentColumn.CaseValueId), IEmployeeCaseDocumentRepository;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

public class CompanyCaseDocumentRepository() : CaseDocumentRepository(Tables.CompanyCaseDocument,
    CompanyCaseDocumentColumn.CaseValueId), ICompanyCaseDocumentRepository;
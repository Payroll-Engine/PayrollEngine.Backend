using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class CompanyCaseDocumentRepository() : CaseDocumentRepository(DbSchema.Tables.CompanyCaseDocument,
    DbSchema.CompanyCaseDocumentColumn.CaseValueId), ICompanyCaseDocumentRepository;
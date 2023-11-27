using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class GlobalCaseDocumentRepository() : CaseDocumentRepository(DbSchema.Tables.GlobalCaseDocument,
    DbSchema.GlobalCaseDocumentColumn.CaseValueId), IGlobalCaseDocumentRepository;
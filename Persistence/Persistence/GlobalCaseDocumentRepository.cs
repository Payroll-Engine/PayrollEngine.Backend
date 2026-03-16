using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

public class GlobalCaseDocumentRepository() : CaseDocumentRepository(Tables.GlobalCaseDocument,
    GlobalCaseDocumentColumn.CaseValueId), IGlobalCaseDocumentRepository;
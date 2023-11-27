using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class NationalCaseDocumentRepository() : CaseDocumentRepository(DbSchema.Tables.NationalCaseDocument,
    DbSchema.NationalCaseDocumentColumn.CaseValueId), INationalCaseDocumentRepository;
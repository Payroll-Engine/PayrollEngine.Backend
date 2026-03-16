using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

public class NationalCaseDocumentRepository() : CaseDocumentRepository(Tables.NationalCaseDocument,
    NationalCaseDocumentColumn.CaseValueId), INationalCaseDocumentRepository;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class NationalCaseValueChangeRepository() : CaseValueChangeRepository(DbSchema.Tables.NationalCaseValueChange,
    DbSchema.NationalCaseValueChangeColumn.CaseChangeId), INationalCaseValueChangeRepository;
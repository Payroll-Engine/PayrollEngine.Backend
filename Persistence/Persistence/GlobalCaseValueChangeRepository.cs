using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class GlobalCaseValueChangeRepository() : CaseValueChangeRepository(DbSchema.Tables.GlobalCaseValueChange,
    DbSchema.GlobalCaseValueChangeColumn.CaseChangeId), IGlobalCaseValueChangeRepository;
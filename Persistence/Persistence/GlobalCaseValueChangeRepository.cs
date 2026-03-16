using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

public class GlobalCaseValueChangeRepository() : CaseValueChangeRepository(Tables.GlobalCaseValueChange,
    GlobalCaseValueChangeColumn.CaseChangeId), IGlobalCaseValueChangeRepository;
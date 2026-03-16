using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

public class NationalCaseValueChangeRepository() : CaseValueChangeRepository(Tables.NationalCaseValueChange,
    NationalCaseValueChangeColumn.CaseChangeId), INationalCaseValueChangeRepository;
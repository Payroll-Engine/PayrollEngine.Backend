using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

public class CompanyCaseValueChangeRepository() : CaseValueChangeRepository(Tables.CompanyCaseValueChange,
    CompanyCaseValueChangeColumn.CaseChangeId), ICompanyCaseValueChangeRepository;
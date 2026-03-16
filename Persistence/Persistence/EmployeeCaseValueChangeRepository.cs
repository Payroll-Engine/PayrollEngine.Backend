using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

public class EmployeeCaseValueChangeRepository() : CaseValueChangeRepository(Tables.EmployeeCaseValueChange,
    EmployeeCaseValueChangeColumn.CaseChangeId), IEmployeeCaseValueChangeRepository;
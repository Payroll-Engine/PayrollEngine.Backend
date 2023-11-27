using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class EmployeeCaseValueChangeRepository() : CaseValueChangeRepository(DbSchema.Tables.EmployeeCaseValueChange,
    DbSchema.EmployeeCaseValueChangeColumn.CaseChangeId), IEmployeeCaseValueChangeRepository;
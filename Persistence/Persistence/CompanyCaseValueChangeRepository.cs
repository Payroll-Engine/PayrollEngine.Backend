using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class CompanyCaseValueChangeRepository() : CaseValueChangeRepository(DbSchema.Tables.CompanyCaseValueChange,
    DbSchema.CompanyCaseValueChangeColumn.CaseChangeId), ICompanyCaseValueChangeRepository;
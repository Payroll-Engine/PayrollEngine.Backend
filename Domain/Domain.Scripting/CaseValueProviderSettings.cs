using System;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Scripting;

public class CaseValueProviderSettings
{
    public IDbContext DbContext { get; set; }
    public IFunctionHost FunctionHost { get; set; }
    public Tenant Tenant { get; set; }
    public ICaseRepository CaseRepository { get; set; }
    public IPayrollCalculator Calculator { get; set; }
    public CaseFieldProvider CaseFieldProvider { get; set; }
    public IRegulationLookupProvider RegulationLookupProvider { get; set; }
    public DatePeriod EvaluationPeriod { get; set; }
    public DateTime EvaluationDate { get; set; }
    public DateTime? RetroDate { get; set; }
}
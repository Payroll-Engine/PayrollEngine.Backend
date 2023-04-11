using System.Collections.Generic;

namespace PayrollEngine.Persistence;

internal sealed class CaseValueQuery
{
    internal int ParentId { get; set; }
    internal int? EmployeeId { get; set; }
    internal string StoredProcedure { get; set; }
    internal string Query { get; set; }
    internal ICollection<string> QueryAttributes { get; set; }
    internal Language? Language { get; set; }
}
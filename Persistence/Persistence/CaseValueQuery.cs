using System.Collections.Generic;

namespace PayrollEngine.Persistence;

internal sealed class CaseValueQuery
{
    internal int ParentId { get; init; }
    internal int? EmployeeId { get; init; }
    internal string StoredProcedure { get; init; }
    internal string Query { get; init; }
    internal ICollection<string> QueryAttributes { get; init; }
    internal string Culture { get; init; }
}
using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace PayrollEngine.Persistence;

public class DbParameterCollection : DynamicParameters
{
    public List<string> GetNames() =>
        ParameterNames.ToList();

    public bool HasAny => ParameterNames.Any();
}
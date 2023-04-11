using System;
using System.Data;

namespace PayrollEngine.Persistence;

internal abstract class DomainRepositoryCommandBase
{
    internal IDbConnection Connection { get; }

    protected DomainRepositoryCommandBase(IDbConnection connection)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }
}
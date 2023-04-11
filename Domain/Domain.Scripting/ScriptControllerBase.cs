using System;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting;

/// <inheritdoc />
public abstract class ScriptControllerBase<TDomain> : IScriptController<TDomain>
    where TDomain : IDomainObject, IScriptObject
{
    public Type DomainType => typeof(TDomain);
}
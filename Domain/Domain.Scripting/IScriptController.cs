using System;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Payroll function controller
/// </summary>
// ReSharper disable once UnusedTypeParameter
public interface IScriptController<out TDomain> where TDomain : IDomainObject
{
    /// <summary>
    /// The script domain type (satisfy NDepend)
    /// </summary>
    Type DomainType { get; }
}
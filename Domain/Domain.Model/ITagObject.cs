
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Object containing tags
/// </summary>
public interface ITagObject : IDomainObject
{
    /// <summary>Object tags</summary>
    List<string> Tags { get; set; }
}
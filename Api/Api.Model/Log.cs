using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The log API object
/// </summary>
public class Log : ApiObjectBase
{
    /// <summary>
    /// The log level (immutable)
    /// </summary>
    public LogLevel Level { get; set; }

    /// <summary>
    /// The log message name (immutable)
    /// </summary>
    public string Message { get; set; }
        
    /// <summary>
    /// The log user (immutable)
    /// </summary>
    public string User { get; set; }

    /// <summary>
    /// The log error (immutable)
    /// </summary>
    public string Error { get; set; }

    /// <summary>
    /// The log comment (immutable)
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    /// The log owner (immutable)
    /// </summary>
    [StringLength(128)]
    public string Owner { get; set; }

    /// <summary>
    /// The log owner type (immutable)
    /// </summary>
    [StringLength(128)]
    public string OwnerType { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Level}: {Message} {base.ToString()}";
}
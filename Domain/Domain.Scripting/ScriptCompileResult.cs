using System;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Script compile result
/// </summary>
public class ScriptCompileResult
{
    /// <summary>
    /// The generated script source code
    /// </summary>
    public string Script { get; }

    /// <summary>
    /// The generated bits
    /// </summary>
    public byte[] Binary { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptCompileResult"/> class
    /// </summary>
    /// <param name="script">The script</param>
    /// <param name="binary">The binary</param>
    public ScriptCompileResult(string script, byte[] binary)
    {
        Script = !string.IsNullOrWhiteSpace(script) ? script : throw new ArgumentException(nameof(script));
        Binary = binary ?? throw new ArgumentNullException(nameof(binary));
    }
}
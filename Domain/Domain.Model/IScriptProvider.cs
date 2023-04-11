using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A script provider
/// </summary>
public interface IScriptProvider
{
    /// <summary>
    /// Get script binary
    /// </summary>
    Task<byte[]> GetBinaryAsync(IScriptObject scriptObject);
}
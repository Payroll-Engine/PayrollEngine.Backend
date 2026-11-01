namespace PayrollEngine.Domain.Model;

/// <summary>
/// Protects webhook secrets for storage (encryption at rest)
/// </summary>
public interface IWebhookSecretProtector
{
    /// <summary>Protect a plain secret for storage</summary>
    /// <param name="secret">The plain secret</param>
    /// <returns>The protected secret</returns>
    string Protect(string secret);

    /// <summary>Restore the plain secret from a protected secret</summary>
    /// <param name="protectedSecret">The protected secret</param>
    /// <returns>The plain secret</returns>
    string Unprotect(string protectedSecret);
}

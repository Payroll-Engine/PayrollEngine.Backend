namespace PayrollEngine.Api.Model;

/// <summary>CORS runtime information</summary>
public class BackendCorsInformation
{
    /// <summary>Whether CORS is active (at least one allowed origin configured)</summary>
    public bool IsActive { get; init; }

    /// <summary>Allowed origins — empty when CORS is inactive</summary>
    public string[] AllowedOrigins { get; init; }
}

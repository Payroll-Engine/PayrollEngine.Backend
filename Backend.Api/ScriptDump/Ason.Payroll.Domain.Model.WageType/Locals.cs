/* Locals */

namespace Ason.Payroll.Client.Scripting.Local;

/// <summary>Switzerland country tools</summary>
public static class Switzerland
{
    /// <summary>Check numeric strings for the swiss UID/BFS (modulus 11, radix 1)</summary>
    public static bool IsValidUidBfs(string value) =>
        new CheckDigit(11, 1, false, "0123456789").IsValid(value);

    /// <summary>Check numeric strings for the swiss BUR number (modulus 11, radix 2)</summary>
    public static bool IsValidBur(string value) =>
        new CheckDigit(11, 2, false, "9876543210").IsValid(value);

    /// <summary>Check numeric strings for the swiss social security number (modulus 13, radix 6)</summary>
    public static bool IsValidSocialSecurity(string value) =>
        CheckDigit.IsValidEan13(value);
}

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Distribution value
/// </summary>
public class DistributionValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DistributionValue"/> class
    /// </summary>
    /// <param name="distributionKey">The distribution key</param>
    /// <param name="factorKey">The factor key</param>
    /// <param name="value">The value</param>
    public DistributionValue(string distributionKey, string factorKey, decimal value)
    {
        DistributionKey = distributionKey;
        FactorKey = factorKey;
        Value = value;
    }

    /// <summary>
    /// The distribution key
    /// </summary>
    public string DistributionKey { get; }

    /// <summary>
    /// The factor key
    /// </summary>
    public string FactorKey { get; }

    /// <summary>
    /// The distribution value
    /// </summary>
    public decimal Value { get; }

    /// <inheritdoc />
    public override string ToString() =>
        $"{DistributionKey}.{FactorKey}={Value}";
}
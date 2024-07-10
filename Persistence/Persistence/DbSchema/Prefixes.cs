
namespace PayrollEngine.Persistence.DbSchema;

public static class Prefixes
{
    /// <summary>Prefix for text attribute fields</summary>
    private static readonly string TextAttributePrefix = SystemSpecification.TextAttributePrefix;

    /// <summary>Prefix for date attribute fields</summary>
    private static readonly string DateAttributePrefix = SystemSpecification.DateAttributePrefix;

    /// <summary>Prefix for numeric attribute fields</summary>
    private static readonly string NumericAttributePrefix = SystemSpecification.NumericAttributePrefix;

    /// <summary>Prefix for boolean attribute fields</summary>
    public static string[] AttributePrefixes =>
    [
        TextAttributePrefix,
        DateAttributePrefix,
        NumericAttributePrefix
    ];
}
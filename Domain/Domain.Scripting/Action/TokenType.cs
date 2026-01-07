namespace PayrollEngine.Domain.Scripting.Action;

/// <summary>
/// Action token type
/// </summary>
internal enum TokenType
{
    /// <summary>
    /// Read only property action token
    /// </summary>
    ReadProperty,

    /// <summary>
    /// Read write property action token
    /// </summary>
    ReadWriteProperty,

    /// <summary>
    /// Method action token
    /// </summary>
    Method,

    /// <summary>
    /// Condition token
    /// </summary>
    Condition,

    /// <summary>
    /// Condition true token
    /// </summary>
    ConditionTrue,

    /// <summary>
    /// Condition false token
    /// </summary>
    ConditionFalse
}
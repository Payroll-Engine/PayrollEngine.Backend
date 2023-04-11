namespace PayrollEngine.Domain.Model;

/// <summary>
/// Action issue info
/// </summary>
public class ActionIssueInfo
{
    /// <summary>The action issue name</summary>
    public string Name { get; set; }

    /// <summary>The action issue message</summary>
    public string Message { get; set; }

    /// <summary>The action issue description</summary>
    public int ParameterCount { get; set; }

    /// <inheritdoc />
    public override string ToString() => Name;
}

/* PayrollResults */
using System;
using System.Collections.Generic;

namespace Ason.Payroll.Client.Scripting;

/// <summary>Case value result</summary>
public class CaseValueResult
{
    /// <summary>The case field name</summary>
    public string CaseFieldName { get; set; }

    /// <summary>The case slot</summary>
    public string CaseSlot { get; set; }

    /// <summary>The case result value</summary>
    public PayrollValue Value { get; set; }

    /// <summary>The case value result tags</summary>
    public List<string> Tags { get; set; }

    /// <summary>Case value result to decimal</summary>
    public static implicit operator PayrollValue(CaseValueResult result) =>
        result.Value;
}

/// <summary>Collector result</summary>
public class CollectorResult
{
    /// <summary>The collector name</summary>
    public string CollectorName { get; set; }

    /// <summary>The result period start</summary>
    public DateTime Start { get; set; }

    /// <summary>The result period end</summary>
    public DateTime End { get; set; }

    /// <summary>The collector result value</summary>
    public decimal Value { get; set; }

    /// <summary>The collector result tags</summary>
    public List<string> Tags { get; set; }

    /// <summary>The result attributes</summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>Convert collector result to decimal</summary>
    public static implicit operator decimal(CollectorResult result) =>
        result.Value;

    /// <summary>Convert collector result to nullable decimal</summary>
    public static implicit operator decimal?(CollectorResult result) =>
        result.Value;
}

/// <summary>Collector custom result</summary>
public class CollectorCustomResult
{
    /// <summary>The collector name</summary>
    public string CollectorName { get; set; }

    /// <summary>The value source</summary>
    public string Source { get; set; }

    /// <summary>The result period start</summary>
    public DateTime Start { get; set; }

    /// <summary>The result period end</summary>
    public DateTime End { get; set; }

    /// <summary>The collector custom result value</summary>
    public decimal Value { get; set; }

    /// <summary>The collector custom result tags</summary>
    public List<string> Tags { get; set; }

    /// <summary>The result attributes</summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>Convert collector custom result to decimal</summary>
    public static implicit operator decimal(CollectorCustomResult result) =>
        result.Value;

    /// <summary>Convert collector custom result to nullable decimal</summary>
    public static implicit operator decimal?(CollectorCustomResult result) =>
        result.Value;
}

/// <summary>Wage type result</summary>
public class WageTypeResult
{
    /// <summary>The wage type number</summary>
    public decimal WageTypeNumber { get; set; }

    /// <summary>The wage type name</summary>
    public string WageTypeName { get; set; }

    /// <summary>The result period start</summary>
    public DateTime Start { get; set; }

    /// <summary>The result period end</summary>
    public DateTime End { get; set; }

    /// <summary>The wage type result value</summary>
    public decimal Value { get; set; }

    /// <summary>The wage type result tags</summary>
    public List<string> Tags { get; set; }

    /// <summary>The result attributes</summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>Convert wage type result to decimal</summary>
    public static implicit operator decimal(WageTypeResult result) =>
        result.Value;

    /// <summary>Convert wage type result to nullable decimal</summary>
    public static implicit operator decimal?(WageTypeResult result) =>
        result.Value;
}

/// <summary>Wage type custom result</summary>
public class WageTypeCustomResult
{
    /// <summary>The wage type number</summary>
    public decimal WageTypeNumber { get; set; }

    /// <summary>The wage type name</summary>
    public string Name { get; set; }

    /// <summary>The value source</summary>
    public string Source { get; set; }

    /// <summary>The result period start</summary>
    public DateTime Start { get; set; }

    /// <summary>The result period end</summary>
    public DateTime End { get; set; }

    /// <summary>The wage type custom result value</summary>
    public decimal Value { get; set; }

    /// <summary>The wage type custom result tags</summary>
    public List<string> Tags { get; set; }

    /// <summary>The result attributes</summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>Convert wage type custom result to decimal</summary>
    public static implicit operator decimal(WageTypeCustomResult result) =>
        result.Value;

    /// <summary>Convert wage type custom result to nullable decimal</summary>
    public static implicit operator decimal?(WageTypeCustomResult result) =>
        result.Value;
}

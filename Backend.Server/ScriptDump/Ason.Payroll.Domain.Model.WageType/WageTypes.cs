/* WageTypes */
using System;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec wage types</summary>
public class WageTypes
{
    /// <summary>The function</summary>
    public Function Function { get; }

    /// <summary>Function constructor</summary>
    public WageTypes(Function function)
    {
        Function = function ?? throw new ArgumentNullException(nameof(function));
    }

    /// <summary>Get slot wage type by slot name</summary>
    /// <param name="baseWageTypeNumber">The wage type base number</param>
    /// <param name="slot">The case slot (numeric text)</param>
    public static decimal GetSlotWageTypeNumber(decimal baseWageTypeNumber, string slot)
    {
        if (!int.TryParse(slot, out var slotNumber))
        {
            throw new ArgumentException($"Invalid case value slot: {slot}");
        }
        return GetSlotWageTypeNumber(baseWageTypeNumber, slotNumber);
    }

    /// <summary>Get slot wage type by slot number</summary>
    /// <param name="baseWageTypeNumber">The wage type base number</param>
    /// <param name="slotNumber">The case slot number</param>
    public static decimal GetSlotWageTypeNumber(decimal baseWageTypeNumber, int slotNumber)
    {
        if (slotNumber <= 0 || slotNumber > Specification.MaxEmployeeInsuranceCodes)
        {
            throw new ArgumentOutOfRangeException(nameof(slotNumber));
        }

        var wageTypeNumber = baseWageTypeNumber + slotNumber.NumberAsDecimalPart();
        return wageTypeNumber;
    }
}

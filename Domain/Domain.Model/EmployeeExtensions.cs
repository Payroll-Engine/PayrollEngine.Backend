using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Extension methods for <see cref="Employee"/>
/// </summary>
public static class EmployeeExtensions
{
    /// <summary>
    /// Test if an employee is in a division
    /// </summary>
    /// <param name="employee">The employee</param>
    /// <param name="divisionName">The name of the division</param>
    /// <returns>True if the employee is associated with the division, otherwise false</returns>
    public static bool InDivision(this Employee employee, string divisionName)
    {
        if (string.IsNullOrWhiteSpace(divisionName))
        {
            throw new ArgumentException(nameof(divisionName));
        }
        return employee.Divisions != null && employee.Divisions.Contains(divisionName);
    }
}
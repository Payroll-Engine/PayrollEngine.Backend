using System;
using System.Runtime.CompilerServices;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Extension methods for UTC validation of dates and periods.
/// Throws <see cref="ArgumentException"/> when a value is not in UTC.
/// </summary>
internal static class UtcValidationExtensions
{
    /// <summary>
    /// Validates that the <paramref name="dateTime"/> is in UTC.
    /// </summary>
    /// <param name="dateTime">The date/time to validate.</param>
    /// <param name="paramName">
    /// Automatically populated with the caller's expression via
    /// <see cref="CallerArgumentExpressionAttribute"/>.
    /// </param>
    /// <returns>The unchanged <paramref name="dateTime"/> for fluent usage.</returns>
    /// <exception cref="ArgumentException"><paramref name="dateTime"/> is not UTC.</exception>
    // ReSharper disable once UnusedMethodReturnValue.Global
    internal static DateTime EnsureUtc(this DateTime dateTime,
        [CallerArgumentExpression(nameof(dateTime))] string paramName = null)
    {
        if (!dateTime.IsUtc())
        {
            throw new ArgumentException("Value must be UTC.", paramName);
        }
        return dateTime;
    }

    /// <summary>
    /// Validates that the nullable <paramref name="dateTime"/> is either <c>null</c> or in UTC.
    /// </summary>
    /// <param name="dateTime">The date/time to validate.</param>
    /// <param name="paramName">
    /// Automatically populated with the caller's expression via
    /// <see cref="CallerArgumentExpressionAttribute"/>.
    /// </param>
    /// <returns>The unchanged <paramref name="dateTime"/> for fluent usage.</returns>
    /// <exception cref="ArgumentException">The value is non-null and not UTC.</exception>
    // ReSharper disable once UnusedMethodReturnValue.Global
    internal static DateTime? EnsureUtc(this DateTime? dateTime,
        [CallerArgumentExpression(nameof(dateTime))] string paramName = null)
    {
        if (dateTime.HasValue && !dateTime.Value.IsUtc())
        {
            throw new ArgumentException("Value must be UTC.", paramName);
        }
        return dateTime;
    }

    /// <summary>
    /// Validates that the <paramref name="period"/> is in UTC.
    /// </summary>
    /// <param name="period">The date period to validate.</param>
    /// <param name="paramName">
    /// Automatically populated with the caller's expression via
    /// <see cref="CallerArgumentExpressionAttribute"/>.
    /// </param>
    /// <returns>The unchanged <paramref name="period"/> for fluent usage.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="period"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="period"/> is not UTC.</exception>
    // ReSharper disable once UnusedMethodReturnValue.Global
    internal static DatePeriod EnsureUtc(this DatePeriod period,
        [CallerArgumentExpression(nameof(period))] string paramName = null)
    {
        ArgumentNullException.ThrowIfNull(period, paramName);
        if (!period.IsUtc)
        {
            throw new ArgumentException("Period must be UTC.", paramName);
        }
        return period;
    }
}

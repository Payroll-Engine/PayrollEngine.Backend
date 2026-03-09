using System;
using System.Globalization;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Calendar = PayrollEngine.Domain.Model.Calendar;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Thread-safe cache for <see cref="IPayrollCalculator"/> instances, keyed by
/// a composite of calendar name and culture. Uses <see cref="Lazy{T}"/> with
/// <see cref="LazyThreadSafetyMode.ExecutionAndPublication"/> to guarantee the
/// factory runs exactly once per cache key, even under parallel employee processing.
/// Failed entries are evicted so transient errors do not permanently poison the cache.
/// </summary>
internal sealed class PayrollCalculatorCache
{
    private readonly ConcurrentDictionary<string, Lazy<IPayrollCalculator>> cache = new();
    private readonly Calendar defaultCalendar = new();

    private IDbContext DbContext { get; }
    private ICalendarRepository CalendarRepository { get; }
    private IPayrollCalculatorProvider CalculatorProvider { get; }

    internal PayrollCalculatorCache(
        IDbContext dbContext,
        ICalendarRepository calendarRepository,
        IPayrollCalculatorProvider calculatorProvider)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        CalendarRepository = calendarRepository ?? throw new ArgumentNullException(nameof(calendarRepository));
        CalculatorProvider = calculatorProvider ?? throw new ArgumentNullException(nameof(calculatorProvider));
    }

    /// <summary>
    /// Returns a <see cref="IPayrollCalculator"/> for the given culture and calendar,
    /// creating and caching it on first access.
    /// </summary>
    /// <param name="tenantId">Tenant owning the calendar.</param>
    /// <param name="userId">User context for the calculator.</param>
    /// <param name="culture">Optional culture name; defaults to the current system culture.</param>
    /// <param name="calendarName">Optional calendar name; defaults to the built-in default calendar.</param>
    /// <returns>A cached or newly created <see cref="IPayrollCalculator"/>.</returns>
    /// <exception cref="PayrollException">Thrown when the specified calendar does not exist.</exception>
    /// <remarks>
    /// The cache key is a composite of <paramref name="calendarName"/> and <paramref name="culture"/>
    /// because <see cref="IPayrollCalculator"/> instances are culture-sensitive (date/number formatting).
    /// The calendar DB lookup is only performed on a cache miss (slow path).
    /// </remarks>
    internal async Task<IPayrollCalculator> GetAsync(int tenantId, int userId,
        string culture = null, string calendarName = null)
    {
        // normalize inputs for composite cache key
        var effectiveCalendarName = string.IsNullOrWhiteSpace(calendarName) ? ".default" : calendarName;
        var effectiveCulture = culture ?? CultureInfo.CurrentCulture.Name;
        var cacheKey = $"{effectiveCalendarName}|{effectiveCulture}";

        // fast path: already cached - no DB call needed
        if (cache.TryGetValue(cacheKey, out var existing))
        {
            try
            {
                return existing.Value;
            }
            catch
            {
                cache.TryRemove(cacheKey, out _);
                throw;
            }
        }

        // slow path: load calendar from DB (only on cache miss)
        var calendar = defaultCalendar;
        if (!string.IsNullOrWhiteSpace(calendarName))
        {
            calendar = await CalendarRepository.GetByNameAsync(DbContext, tenantId, calendarName);
            if (calendar == null)
            {
                throw new PayrollException($"Unknown calendar {calendarName}.");
            }
        }

        var cultureInfo = new CultureInfo(effectiveCulture);

        // Lazy<T> with ExecutionAndPublication guarantees the factory is called exactly once,
        // even under parallel contention across multiple employee processing threads.
        var lazy = cache.GetOrAdd(cacheKey,
            _ => new Lazy<IPayrollCalculator>(
                () => CalculatorProvider.CreateCalculator(
                    tenantId: tenantId,
                    userId: userId,
                    culture: cultureInfo,
                    calendar: calendar),
                LazyThreadSafetyMode.ExecutionAndPublication));

        // evict the entry on failure so transient errors do not permanently poison the cache
        try
        {
            return lazy.Value;
        }
        catch
        {
            cache.TryRemove(cacheKey, out _);
            throw;
        }
    }
}

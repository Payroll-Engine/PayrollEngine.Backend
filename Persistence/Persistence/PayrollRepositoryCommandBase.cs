using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal abstract class PayrollRepositoryCommandBase(IDbContext dbContext)
{
    internal IDbContext DbContext { get; } = dbContext ?? throw new ArgumentNullException(nameof(dbContext));


    protected static void ApplyOverrideFilter<TKey, TBuild>(IEnumerable<IGrouping<TKey, TBuild>> derivedItemsByKey,
        IList sourceItems, IList<TBuild> buildItems, OverrideType overrideType)
        where TBuild : IDerivableObject
    {
        foreach (var derivedItem in derivedItemsByKey)
        {
            var mostDerivedItem = derivedItem.First();

            // mismatching override type
            if (mostDerivedItem.OverrideType != overrideType)
            {
                // remove all derived items
                foreach (var item in derivedItem)
                {
                    sourceItems.Remove(item);
                    buildItems.Remove(item);
                }
            }
        }
    }

    protected static void ApplyOverrideFilter<TKey, TBuild>(IEnumerable<IGrouping<TKey, TBuild>> derivedItemsByKey,
        IList<TBuild> buildItems, OverrideType overrideType)
        where TBuild : IDerivableObject
    {
        foreach (var derivedItem in derivedItemsByKey)
        {
            var mostDerivedItem = derivedItem.First();

            // mismatching override type
            if (mostDerivedItem.OverrideType != overrideType)
            {
                // remove all derived items
                foreach (var item in derivedItem)
                {
                    buildItems.Remove(item);
                }
            }
        }
    }

    protected static TValue CollectDerivedValue<TItem, TValue>(IEnumerable<TItem> items, Func<TItem, TValue> valueFunction) =>
        items.Select(valueFunction).FirstOrDefault(itemValue => !Equals(itemValue, default(TValue)));

    protected static List<TValue> CollectDerivedList<TItem, TValue>(IEnumerable<TItem> items, Func<TItem, IEnumerable<TValue>> valueFunction)
    {
        var values = new List<TValue>();
        foreach (var item in items)
        {
            var itemValues = valueFunction(item);
            if (itemValues != null)
            {
                values.AddRange(itemValues);
            }
        }
        return values.Any() ? values : null;
    }

    protected static Dictionary<string, object> CollectDerivedAttributes(IEnumerable<IAttributeObject> items) =>
        CollectDerivedDictionary(items, x => x.Attributes);

    protected static Dictionary<TKey, TValue> CollectDerivedDictionary<TItem, TKey, TValue>(IEnumerable<TItem> items,
        Func<TItem, IDictionary<TKey, TValue>> valueFunction)
    {
        var values = new Dictionary<TKey, TValue>();
        foreach (var item in items)
        {
            var itemValues = valueFunction(item);
            if (itemValues != null)
            {
                foreach (var itemValue in itemValues)
                {
                    values[itemValue.Key] = itemValue.Value;
                }
            }
        }
        return values.Any() ? values : null;
    }
}
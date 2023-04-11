using System;
using System.Collections.Generic;
using System.Linq;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Class AttributeObjectExtensions
/// </summary>
public static class AttributeObjectExtensions
{
    /// <summary>
    /// Collect derived attributes
    /// </summary>
    /// <param name="objects">The derived object, ordered from the most-derived to the root object</param>
    /// <param name="attributesFunction">The attributes function</param>
    /// <returns>The case field name of the duplicate case, null if no duplicate is present</returns>
    public static Dictionary<string, object> CollectDerivedAttributes<T>(this IList<T> objects, Func<T, Dictionary<string, object>> attributesFunction)
    {
        if (objects == null || objects.Count == 0)
        {
            return null;
        }

        var derivedAttributes = new Dictionary<string, object>();
        // reverse order: from root to the most derived
        for (var i = objects.Count - 1; i >= 0; i--)
        {
            var attributes = attributesFunction(objects[i]);
            if (attributes == null || !attributes.Any())
            {
                continue;
            }
            foreach (var attribute in attributes)
            {
                // add or replace/overwrite attribute
                derivedAttributes[attribute.Key] = attribute.Value;
            }
        }
        return derivedAttributes;
    }
}
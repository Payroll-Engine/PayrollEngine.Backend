//#define COLLECTED_RESULTS

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal abstract class ResultCommandBase
{
    internal IDbConnection Connection { get; }

    protected ResultCommandBase(IDbConnection connection)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <summary>Apply the tag filter</summary>
    /// <param name="tagObjects">The tagged objects</param>
    /// <param name="tags">The filter tags</param>
    /// <remarks>keep in sync with client scripting ConsolidatedResultCache</remarks>
    /// <returns>Collection with objects matching the tag filter</returns>
    protected static IEnumerable<T> ApplyTagFilter<T>(IEnumerable<T> tagObjects, IEnumerable<string> tags = null)
        where T : class, ITagObject
    {
        // tags
        if (tags == null)
        {
            return tagObjects;
        }
        var tagList = tags.ToList();
        if (!tagList.Any())
        {
            return tagObjects;
        }

        // first tag used to enable the logical OR mode
        var anyTag = "*".Equals(tagList.First().Trim(), StringComparison.InvariantCultureIgnoreCase);
        if (anyTag)
        {
            tagList.RemoveAt(0);
            // any query only with multiple tags
            anyTag = tagList.Count > 1;
        }
        if (!tagList.Any())
        {
            return tagObjects;
        }

        // apply tags filter
        var filteredObjects = new List<T>();
        foreach (var tagObject in tagObjects)
        {
            if (tagObject.Tags == null)
            {
                continue;
            }

            if (anyTag)
            {
                // check if any tag is present
                if (tagList.Any(x => tagObject.Tags.Contains(x)))
                {
                    filteredObjects.Add(tagObject);
                }
            }
            else
            {
                // check if all tags are present
                if (tagList.All(x => tagObject.Tags.Contains(x)))
                {
                    filteredObjects.Add(tagObject);
                }
            }
        }
        return filteredObjects;
    }

    protected static void QueryBegin()
    {
#if COLLECTED_RESULTS
        Stopwatch.Restart();
#endif
    }

    protected static void QueryEnd(Func<string> message)
    {
#if COLLECTED_RESULTS
        Stopwatch.Stop();
        Log.Information($"{message()}: {Stopwatch.ElapsedMilliseconds} ms");
#endif
    }

    /// <summary>
    /// Get the items as string
    /// </summary>
    /// <param name="items">The items</param>
    protected static string GetItemsString<T>(IEnumerable<T> items) =>
        items == null ? "*" : string.Join(", ", items);

#if COLLECTED_RESULTS
    private static readonly System.Diagnostics.Stopwatch Stopwatch = new();
#endif
}
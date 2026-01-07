using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Extension methods for IEnumerable
/// </summary>
public static class DomainEnumerableExtensions
{
    /// <param name="derivedObjects">The objects sorted from leaf to root</param>
    /// <typeparam name="T">The domain object</typeparam>
    extension<T>(IEnumerable<T> derivedObjects) where T : IDomainObject
    {
        /// <summary>
        /// Get newest derived object
        /// </summary>
        /// <param name="createdBefore">The object creation date</param>
        public T GetNewestObject(DateTime createdBefore)
        {
            T result = default;
            DateTime? created = null;
            foreach (var derivedObject in derivedObjects)
            {
                if (derivedObject.Created < createdBefore)
                {
                    if (!created.HasValue || created.Value < derivedObject.Created)
                    {
                        // newer
                        result = derivedObject;
                        created = derivedObject.Created;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Get derived script object using a script provider
        /// </summary>
        /// <param name="scriptProvider">Object value provider function</param>
        public ICollection<T> GetDerivedExpressionObjects(Func<T, string> scriptProvider)
        {
            var scriptObjects = new List<T>();
            var sealedScript = false;
            foreach (var derivedObject in derivedObjects)
            {
                // retrieve script
                var script = scriptProvider(derivedObject);

                // script available
                if (!string.IsNullOrWhiteSpace(script))
                {
                    // sealed script
                    if (script.IsSealedScript())
                    {
                        // use only the script object closest to the root object
                        scriptObjects.Clear();
                        scriptObjects.Add(derivedObject);
                        sealedScript = true;
                    }
                    else if (!sealedScript)
                    {
                        // derived script
                        scriptObjects.Add(derivedObject);
                    }
                }
            }
            return scriptObjects;
        }
    }
}
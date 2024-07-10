using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PayrollEngine.Api.Core
{
    /// <summary>
    /// API controller visibility conventions
    /// <remarks>source https://joonasw.net/view/hide-actions-from-swagger-openapi-documentation-in-aspnet-core</remarks>
    /// </summary>
    internal sealed class ControllerVisibilityConvention : IActionModelConvention
    {
        private List<string> VisibleControllers { get; }
        private List<string> HiddenControllers { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="visibleControllers">List of visible controller name masks (wildcards: *?)</param>
        /// <param name="hiddenControllers">List of hidden controller name masks (wildcards: *?)</param>
        internal ControllerVisibilityConvention(IEnumerable<string> visibleControllers = null,
            IEnumerable<string> hiddenControllers = null)
        {
            VisibleControllers = visibleControllers != null ? [..visibleControllers] : [];
            HiddenControllers = hiddenControllers != null ? [..hiddenControllers] : [];
        }

        public void Apply(ActionModel action)
        {
            // visible
            if (VisibleControllers.Count > 0)
            {
                action.ApiExplorer.IsVisible =
                    VisibleControllers.Any(x => FitsMask(action.Controller.ControllerName, x));
            }
            // hidden
            else if (HiddenControllers.Count > 0)
            {
                action.ApiExplorer.IsVisible = 
                           !HiddenControllers.Any(x => FitsMask(action.Controller.ControllerName, x));
            }
        }

        private static bool FitsMask(string test, string mask)
        {
            // no mask: simple string compare
            if (!mask.Contains('?') && !mask.Contains('*'))
            {
                return string.Equals(test, mask, StringComparison.InvariantCultureIgnoreCase);
            }

            // regex
            var regex = new Regex(mask.Replace(".", "[.]").Replace("*", ".*").Replace('?', '.'));
            return regex.IsMatch(test);
        }
    }
}

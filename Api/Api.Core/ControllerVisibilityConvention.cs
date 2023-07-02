﻿using Microsoft.AspNetCore.Mvc.ApplicationModels;
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
    internal sealed class ControllerVisibilityConvention : IControllerModelConvention
    {
        private List<string> VisibleControllers { get; }
        private List<string> HiddenControllers { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="visibleControllers">List of visible controller name masks (wildcards: *?)</param>
        /// <param name="hiddenControllers">List of hidden controller name masks (wildcards: *?)</param>
        public ControllerVisibilityConvention(IEnumerable<string> visibleControllers = null,
            IEnumerable<string> hiddenControllers = null)
        {
            VisibleControllers = visibleControllers != null ? new(visibleControllers) : new();
            HiddenControllers = hiddenControllers != null ? new(hiddenControllers) : new();
        }

        public void Apply(ControllerModel controller)
        {
            if (VisibleControllers.Count == 0 && HiddenControllers.Count == 0)
            {
                return;
            }

            // visible
            if (VisibleControllers.Count > 0)
            {
                controller.ApiExplorer.IsVisible =
                    VisibleControllers.Any(x => FitsMask(controller.ControllerName, x));
            }
            // hidden
            else if (HiddenControllers.Count > 0)
            {
                controller.ApiExplorer.IsVisible =
                    !HiddenControllers.Any(x => FitsMask(controller.ControllerName, x));
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

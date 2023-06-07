using System;

namespace PayrollEngine.Api.Core
{
    /// <summary>
    /// The backend server specification
    /// </summary>
    public static class BackendSpecification
    {
        /// <summary>
        /// The default API version
        /// </summary>
        public static Version DefaultApiVersion { get; } = new(1, 0);

        /// <summary>
        /// The current API version
        /// <remarks>Version to change on updates</remarks>
        /// </summary>
        public static Version CurrentApiVersion { get; } = DefaultApiVersion;

        /// <summary>
        /// The API description
        /// </summary>
        public static string ApiDescription { get; } = "Payroll Engine API";

        /// <summary>
        /// The API name
        /// </summary>
        public static string ApiName { get; } = "Payroll Engine Backend API";
    }
}

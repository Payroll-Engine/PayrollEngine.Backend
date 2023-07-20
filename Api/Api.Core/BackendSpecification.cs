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
        public static Version CurrentApiVersion => DefaultApiVersion;

        /// <summary>
        /// The API description
        /// </summary>
        public static string ApiDescription => "Payroll Engine API";

        /// <summary>
        /// The API name
        /// </summary>
        public static string ApiName => "Payroll Engine Backend API";

        /// <summary>Tenant authorization header</summary>
        public static string TenantAuthorizationHeader => "Auth-Tenant";
    }
}

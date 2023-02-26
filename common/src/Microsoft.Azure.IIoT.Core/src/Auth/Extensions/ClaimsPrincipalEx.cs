// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace System.Security.Claims
{
    /// <summary>
    /// Model extensions
    /// </summary>
    public static class ClaimsPrincipalEx
    {
        /// <summary>
        /// Gets the unique object ID associated with the <see cref="ClaimsPrincipal"/>
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="throwIfNotFound"></param>
        /// <returns></returns>
        public static string GetObjectId(this ClaimsPrincipal principal,
            bool throwIfNotFound = true)
        {
            var userIdentifier = principal.FindFirstValue("oid");
            if (string.IsNullOrEmpty(userIdentifier))
            {
                return principal.FindFirstValue(
                "http://schemas.microsoft.com/identity/claims/objectidentifier",
                throwIfNotFound);
            }
            return userIdentifier;
        }

        /// <summary>
        /// Gets the Tenant ID associated with the <see cref="ClaimsPrincipal"/>
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static string GetTenantId(this ClaimsPrincipal principal)
        {
            var tenantId = principal.FindFirstValue("tid");
            if (string.IsNullOrEmpty(tenantId))
            {
                return principal.FindFirstValue(
                    "http://schemas.microsoft.com/identity/claims/tenantid");
            }

            return tenantId;
        }

        /// <summary>
        /// Gets the login-hint associated with a <see cref="ClaimsPrincipal"/>
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static string GetLoginHint(this ClaimsPrincipal principal)
        {
            return GetDisplayName(principal);
        }

        /// <summary>
        /// Get the display name for the signed-in <see cref="ClaimsPrincipal"/>
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static string GetDisplayName(this ClaimsPrincipal principal)
        {
            var displayName = principal.FindFirstValue("preferred_username");
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = principal.FindFirstValue(ClaimsIdentity.DefaultNameClaimType);
                if (string.IsNullOrWhiteSpace(displayName))
                {
                    displayName = principal.FindFirstValue("name");
                }
            }
            return displayName;
        }

        /// <summary>
        /// Gets the user flow id associated with the <see cref="ClaimsPrincipal"/>
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static string GetUserFlowId(this ClaimsPrincipal principal)
        {
            var userFlowId = principal.FindFirstValue("tfp");
            if (string.IsNullOrEmpty(userFlowId))
            {
                return principal.FindFirstValue(
                    "http://schemas.microsoft.com/claims/authnclassreference");
            }
            return userFlowId;
        }

        /// <summary>
        /// Gets the NameIdentifierId associated with the <see cref="ClaimsPrincipal"/>
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static string GetNameIdentifierId(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue("utid");
        }

        /// <summary>
        /// Helper to return a claim value for a <see cref="ClaimsPrincipal"/>
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="claimType"></param>
        /// <param name="throwIfNotFound"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static string FindFirstValue(this ClaimsPrincipal principal,
            string claimType, bool throwIfNotFound = false)
        {
            var value = principal.FindFirst(claimType)?.Value;
            if (throwIfNotFound && string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    $"The supplied principal does not contain a claim of type {claimType}");
            }
            return value ?? string.Empty;
        }
    }
}

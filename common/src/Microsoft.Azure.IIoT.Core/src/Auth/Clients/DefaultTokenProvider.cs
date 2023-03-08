﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Auth.Clients
{
    using Microsoft.Azure.IIoT.Auth;
    using Microsoft.Azure.IIoT.Auth.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Enumerates all token sources and provides token from first successful source
    /// </summary>
    public class DefaultTokenProvider : ITokenProvider
    {
        /// <inheritdoc/>
        public DefaultTokenProvider(IEnumerable<ITokenSource> tokenSources)
        {
            _tokenSources = tokenSources?.Where(s => s.IsEnabled).ToList() ??
                throw new ArgumentNullException(nameof(tokenSources));
        }

        /// <inheritdoc/>
        public bool Supports(string resource)
        {
            return _tokenSources.Any(p => p.Resource == resource);
        }

        /// <inheritdoc/>
        public virtual async Task<TokenResultModel> GetTokenForAsync(
            string resource, IEnumerable<string> scopes = null)
        {
            foreach (var source in _tokenSources.Where(p => p.Resource == resource))
            {
                try
                {
                    var token = await source.GetTokenAsync(scopes).ConfigureAwait(false);
                    if (token != null)
                    {
                        return token;
                    }
                }
                catch
                {
                    continue;
                }
            }
            return null;
        }

        /// <inheritdoc/>
        public virtual async Task InvalidateAsync(string resource)
        {
            await Task.WhenAll(_tokenSources
                .Where(p => p.Resource == resource)
                .Select(p => p.InvalidateAsync())).ConfigureAwait(false);
        }

        private readonly List<ITokenSource> _tokenSources;
    }
}

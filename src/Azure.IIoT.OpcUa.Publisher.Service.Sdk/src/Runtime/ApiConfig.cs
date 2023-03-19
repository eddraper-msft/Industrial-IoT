﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Azure.IIoT.OpcUa.Publisher.Service.Sdk.Runtime
{
    using Microsoft.Azure.IIoT;
    using Microsoft.Azure.IIoT.Messaging.SignalR;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    /// <summary>
    /// Complete api configuration
    /// </summary>
    public class ApiConfig : ConfigBase, IServiceApiConfig,
        ISignalRClientConfig
    {
        /// <summary>
        /// Configuration keys
        /// </summary>
        private const string kServiceUrlKey = "ServiceUrl";

        /// <inheritdoc/>
        public bool UseMessagePackProtocol => false;

        /// <inheritdoc/>
        public string ServiceUrl => GetStringOrDefault(kServiceUrlKey,
            () => GetStringOrDefault(PcsVariable.PCS_PUBLISHER_SERVICE_URL,
                () => GetDefaultUrl("9045", "publisher")));

        /// <inheritdoc/>
        public Func<Task<string>> TokenProvider { get; set; }

        /// <inheritdoc/>
        public ApiConfig(IConfiguration configuration) :
            base(configuration)
        {
        }

        /// <summary>
        /// Get endpoint url
        /// </summary>
        /// <param name="port"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        protected string GetDefaultUrl(string port, string path)
        {
            var cloudEndpoint = GetStringOrDefault(PcsVariable.PCS_SERVICE_URL)?.Trim()?.TrimEnd('/');
            if (string.IsNullOrEmpty(cloudEndpoint))
            {
                // Test port is open
                if (!int.TryParse(port, out var nPort))
                {
                    return $"http://localhost:9080/{path}";
                }
                using (var socket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Unspecified))
                {
                    try
                    {
                        socket.Connect(IPAddress.Loopback, nPort);
                        return $"http://localhost:{port}";
                    }
                    catch
                    {
                        return $"http://localhost:9080/{path}";
                    }
                }
            }
            return $"{cloudEndpoint}/{path}";
        }
    }
}

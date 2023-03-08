// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Azure.IIoT.OpcUa.Services.Handlers
{
    using Furly.Extensions.Utils;
    using Furly.Azure.IoT;
    using Microsoft.Azure.IIoT.Messaging;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;
    using Azure.Messaging.EventHubs;
    using Microsoft.Azure.Amqp.Framing;

    /// <summary>
    /// Default iot hub device event handler implementation
    /// </summary>
    public sealed class DeviceTelemetryEventHandler : IIoTHubTelemetryHandler
    {
        /// <summary>
        /// Create processor factory
        /// </summary>
        /// <param name="handlers"></param>
        public DeviceTelemetryEventHandler(IEnumerable<IMessageHandler> handlers)
        {
            if (handlers == null)
            {
                throw new ArgumentNullException(nameof(handlers));
            }
            _handlers = new ConcurrentDictionary<string, IMessageHandler>(
                handlers.Select(h => KeyValuePair.Create(h.MessageSchema.ToLowerInvariant(), h)));
        }

        /// <inheritdoc/>
        public async ValueTask HandleAsync(string deviceId, string moduleId, string topic,
            ReadOnlyMemory<byte> data, string contentType, CancellationToken ct)
        {
            if (_handlers.TryGetValue(topic.ToLowerInvariant(), out var handler))
            {
                // TODO: Pass properties down
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!
                await handler.HandleAsync(deviceId, moduleId, data.ToArray(),
                    null, null).ConfigureAwait(false);
            }
            else
            {
                //  TODO: when handling third party OPC UA PubSub Messages
                //  the schemaType might not exist
            }
        }

        private readonly ConcurrentDictionary<string, IMessageHandler> _handlers;
    }
}

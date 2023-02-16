﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Azure.IIoT.OpcUa.Services.Events.v2 {
    using Azure.IIoT.OpcUa.Api.Models;
    using Microsoft.Azure.IIoT.Messaging;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Endpoint registry event publisher
    /// </summary>
    public class EndpointEventBusPublisher : IEndpointRegistryListener {

        /// <summary>
        /// Create event publisher
        /// </summary>
        /// <param name="bus"></param>
        public EndpointEventBusPublisher(IEventBus bus) {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc/>
        public Task OnEndpointDeletedAsync(
            RegistryOperationContextModel context, string endpointId, EndpointInfoModel endpoint) {
            return _bus.PublishAsync(Wrap(EndpointEventType.Deleted, context,
                endpointId, endpoint));
        }

        /// <inheritdoc/>
        public Task OnEndpointDisabledAsync(
            RegistryOperationContextModel context, EndpointInfoModel endpoint) {
            return _bus.PublishAsync(Wrap(EndpointEventType.Disabled, context,
                endpoint.Registration.Id, endpoint));
        }

        /// <inheritdoc/>
        public Task OnEndpointEnabledAsync(
            RegistryOperationContextModel context, EndpointInfoModel endpoint) {
            return _bus.PublishAsync(Wrap(EndpointEventType.Enabled, context,
                endpoint.Registration.Id, endpoint));
        }

        /// <inheritdoc/>
        public Task OnEndpointNewAsync(
            RegistryOperationContextModel context, EndpointInfoModel endpoint) {
            return _bus.PublishAsync(Wrap(EndpointEventType.New, context,
                endpoint.Registration.Id, endpoint));
        }

        /// <summary>
        /// Create endpoint event
        /// </summary>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="endpointId"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        private static EndpointEventModel Wrap(EndpointEventType type,
            RegistryOperationContextModel context, string endpointId,
            EndpointInfoModel endpoint) {
            return new EndpointEventModel {
                EventType = type,
                Context = context,
                Id = endpointId,
                Endpoint = endpoint
            };
        }

        private readonly IEventBus _bus;
    }
}

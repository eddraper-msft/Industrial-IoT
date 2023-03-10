// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Azure.IIoT.OpcUa.Services.Registry
{
    using Azure.IIoT.OpcUa.Services.Registry.Models;
    using Azure.IIoT.OpcUa.Models;
    using Furly.Azure;
    using Furly.Azure.IoT;
    using Furly.Exceptions;
    using Furly.Extensions.Serializers;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Publisher registry which uses the IoT Hub twin services for supervisor
    /// identity management.
    /// </summary>
    public sealed class PublisherRegistry : IPublisherRegistry
    {
        /// <summary>
        /// Create registry services
        /// </summary>
        /// <param name="iothub"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        /// <param name="events"></param>
        public PublisherRegistry(IIoTHubTwinServices iothub, IJsonSerializer serializer,
            ILogger logger, IPublisherRegistryListener events = null)
        {
            _iothub = iothub ?? throw new ArgumentNullException(nameof(iothub));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _events = events;
        }

        /// <inheritdoc/>
        public async Task<PublisherModel> GetPublisherAsync(string publisherId,
            bool onlyServerState, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(publisherId))
            {
                throw new ArgumentNullException(nameof(publisherId));
            }
            if (!HubResource.Parse(publisherId, out _, out var deviceId, out var moduleId, out var error))
            {
                throw new ArgumentException(error, nameof(publisherId));
            }
            var device = await _iothub.GetAsync(deviceId, moduleId, ct).ConfigureAwait(false);
            if (!(device.ToEntityRegistration(onlyServerState) is PublisherRegistration registration))
            {
                throw new ResourceNotFoundException($"{publisherId} is not a publisher registration.");
            }
            return registration.ToPublisherModel();
        }

        /// <inheritdoc/>
        public async Task UpdatePublisherAsync(string publisherId,
            PublisherUpdateModel request, CancellationToken ct)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            if (string.IsNullOrEmpty(publisherId))
            {
                throw new ArgumentNullException(nameof(publisherId));
            }
            if (!HubResource.Parse(publisherId, out _, out var deviceId, out var moduleId, out var error))
            {
                throw new ArgumentException(error, nameof(publisherId));
            }
            while (true)
            {
                try
                {
                    var twin = await _iothub.GetAsync(deviceId, moduleId, ct).ConfigureAwait(false);
                    if (twin.Id != deviceId && twin.ModuleId != moduleId)
                    {
                        throw new ArgumentException("Id must be same as twin to patch",
                            nameof(publisherId));
                    }

                    if (!(twin.ToEntityRegistration(true) is PublisherRegistration registration))
                    {
                        throw new ResourceNotFoundException(
                            $"{publisherId} is not a publisher registration.");
                    }
                    // Update registration from update request
                    var patched = registration.ToPublisherModel();
                    if (request.SiteId != null)
                    {
                        patched.SiteId = string.IsNullOrEmpty(request.SiteId) ?
                            null : request.SiteId;
                    }

                    if (request.LogLevel != null)
                    {
                        patched.LogLevel = request.LogLevel == TraceLogLevel.Information ?
                            null : request.LogLevel;
                    }

                    // Patch
                    twin = await _iothub.PatchAsync(registration.Patch(
                        patched.ToPublisherRegistration(), _serializer), false, ct).ConfigureAwait(false);

                    // Send update to through broker
                    registration = twin.ToEntityRegistration(true) as PublisherRegistration;
                    await (_events?.OnPublisherUpdatedAsync(null, registration.ToPublisherModel())).ConfigureAwait(false);
                    return;
                }
                catch (ResourceOutOfDateException ex)
                {
                    _logger.LogDebug(ex, "Retrying updating supervisor...");
                    continue;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<PublisherListModel> ListPublishersAsync(
            string continuation, bool onlyServerState, int? pageSize, CancellationToken ct)
        {
            const string query = "SELECT * FROM devices.modules WHERE " +
                $"properties.reported.{OpcUa.Constants.TwinPropertyTypeKey} = '{Constants.EntityTypePublisher}' " +
                $"AND NOT IS_DEFINED(tags.{nameof(EntityRegistration.NotSeenSince)})";
            var devices = await _iothub.QueryDeviceTwinsAsync(query, continuation, pageSize, ct).ConfigureAwait(false);
            return new PublisherListModel
            {
                ContinuationToken = devices.ContinuationToken,
                Items = devices.Items
                    .Select(t => t.ToPublisherRegistration(onlyServerState))
                    .Select(s => s.ToPublisherModel())
                    .ToList()
            };
        }

        /// <inheritdoc/>
        public async Task<PublisherListModel> QueryPublishersAsync(
            PublisherQueryModel query, bool onlyServerState, int? pageSize, CancellationToken ct)
        {
            var sql = "SELECT * FROM devices.modules WHERE " +
                $"properties.reported.{OpcUa.Constants.TwinPropertyTypeKey} = '{Constants.EntityTypePublisher}'";

            if (query?.SiteId != null)
            {
                // If site id provided, include it in search
                sql += $"AND (properties.reported.{OpcUa.Constants.TwinPropertySiteKey} = " +
                    $"'{query.SiteId}' OR properties.desired.{OpcUa.Constants.TwinPropertySiteKey} = " +
                    $"'{query.SiteId}' OR deviceId = '{query.SiteId}') ";
            }

            if (query?.Connected != null)
            {
                // If flag provided, include it in search
                if (query.Connected.Value)
                {
                    sql += "AND connectionState = 'Connected' ";
                }
                else
                {
                    sql += "AND connectionState != 'Connected' ";
                }
            }

            var queryResult = await _iothub.QueryDeviceTwinsAsync(sql, null,
                pageSize, ct).ConfigureAwait(false);
            return new PublisherListModel
            {
                ContinuationToken = queryResult.ContinuationToken,
                Items = queryResult.Items
                    .Select(t => t.ToPublisherRegistration(onlyServerState))
                    .Select(s => s.ToPublisherModel())
                    .ToList()
            };
        }

        private readonly IIoTHubTwinServices _iothub;
        private readonly IJsonSerializer _serializer;
        private readonly IPublisherRegistryListener _events;
        private readonly ILogger _logger;
    }
}

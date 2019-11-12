﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.OpcUa.Api.Registry.Models {
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Supervisor event type
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SupervisorEventType {

        /// <summary>
        /// New
        /// </summary>
        New,

        /// <summary>
        /// Updated
        /// </summary>
        Updated,

        /// <summary>
        /// Connected
        /// </summary>
        Connected,

        /// <summary>
        /// Disconnected
        /// </summary>
        Disconnected,

        /// <summary>
        /// Deleted
        /// </summary>
        Deleted,
    }

    /// <summary>
    /// Supervisor event
    /// </summary>
    public class SupervisorEventApiModel {

        /// <summary>
        /// Event type
        /// </summary>
        [JsonProperty(PropertyName = "eventType")]
        public SupervisorEventType EventType { get; set; }

        /// <summary>
        /// Application
        /// </summary>
        [JsonProperty(PropertyName = "supervisor",
            NullValueHandling = NullValueHandling.Ignore)]
        public SupervisorApiModel Supervisor { get; set; }
    }
}
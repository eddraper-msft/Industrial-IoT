// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.EventHub.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Event hub configuration - wraps a configuration root
    /// </summary>
    public class EventHubClientConfig : ConfigBase, IEventHubClientConfig {

        /// <summary>
        /// Event hub configuration
        /// </summary>
        private const string kEventHubConnStringKey = "EventHubConnectionString";
        private const string kEventHubNameKey = "EventHubName";

        /// <summary> Event hub connection string </summary>
        public string EventHubConnString => GetStringOrDefault(kEventHubConnStringKey,
            GetStringOrDefault("PCS_EVENTHUB_CONNSTRING", null));
        /// <summary> Event hub path </summary>
        public string EventHubPath => GetStringOrDefault(kEventHubNameKey,
            GetStringOrDefault("PCS_EVENTHUB_NAME", null));
        /// <summary> Whether use websockets to connect </summary>

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public EventHubClientConfig(IConfiguration configuration) :
            base(configuration) {
        }
    }
}

﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Api.Publisher.Models {
    using Microsoft.Azure.IIoT.OpcUa.Api.Publisher.Models;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Describes event fields to be published
    /// </summary>
    [DataContract]
    public class PublishedDataSetEventApiModel {

        /// <summary>
        /// Identifier of event in the dataset.
        /// </summary>
        [DataMember(Name = "id", Order = 0,
            EmitDefaultValue = false)]
        public string Id { get; set; }

        /// <summary>
        /// Event notifier to subscribe to
        /// </summary>
        [DataMember(Name = "eventNotifier", Order = 1)]
        public string EventNotifier { get; set; }

        /// <summary>
        /// Browse path to event notifier node (Publisher extension)
        /// </summary>
        [DataMember(Name = "browsePath", Order = 2,
            EmitDefaultValue = false)]
        public string[] BrowsePath { get; set; }

        /// <summary>
        /// Event fields to select
        /// </summary>
        [DataMember(Name = "selectClauses", Order = 3)]
        public List<SimpleAttributeOperandApiModel> SelectClauses { get; set; }

        /// <summary>
        /// Filter to use to select event fields
        /// </summary>
        [DataMember(Name = "whereClause", Order = 4)]
        public ContentFilterApiModel WhereClause { get; set; }

        /// <summary>
        /// Queue size (Publisher extension)
        /// </summary>
        [DataMember(Name = "queueSize", Order = 5,
            EmitDefaultValue = false)]
        public uint? QueueSize { get; set; }

        /// <summary>
        /// Discard new values if queue is full (Publisher extension)
        /// </summary>
        [DataMember(Name = "discardNew", Order = 6,
            EmitDefaultValue = false)]
        public bool? DiscardNew { get; set; }

        /// <summary>
        /// Monitoring mode (Publisher extension)
        /// </summary>
        [DataMember(Name = "monitoringMode", Order = 7,
            EmitDefaultValue = false)]
        public MonitoringMode? MonitoringMode { get; set; }

        /// <summary>
        /// Condition handling settings
        /// </summary>
        [DataMember(Name = "conditionHandling", Order = 9)]
        public ConditionHandlingOptionsApiModel ConditionHandling { get; set; }

        /// <summary>
        /// Simple event Type definition id
        /// </summary>
        [DataMember(Name = "typeDefinitionId", Order = 10)]
        public string TypeDefinitionId { get; set; }

        /// <summary>
        /// Event name
        /// </summary>
        [DataMember(Name = "publishedEventName", Order = 11,
            EmitDefaultValue = false)]
        public string PublishedEventName { get; set; }
    }
}
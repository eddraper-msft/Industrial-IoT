// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Azure.IIoT.OpcUa.Publisher.Services
{
    using Azure.IIoT.OpcUa.Publisher;
    using Azure.IIoT.OpcUa.Publisher.Models;
    using Azure.IIoT.OpcUa.Publisher.Stack;
    using Azure.IIoT.OpcUa.Publisher.Stack.Models;
    using Azure.IIoT.OpcUa.Encoders.Models;
    using Azure.IIoT.OpcUa.Encoders.PubSub;
    using Azure.IIoT.OpcUa.Models;
    using Furly.Extensions.Messaging;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Extensions.Logging;
    using Opc.Ua;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Metrics;
    using System.Linq;

    /// <summary>
    /// Creates PubSub encoded messages
    /// </summary>
    public class NetworkMessageEncoder : IMessageEncoder
    {
        /// <inheritdoc/>
        public long NotificationsDroppedCount { get; private set; }

        /// <inheritdoc/>
        public long NotificationsProcessedCount { get; private set; }

        /// <inheritdoc/>
        public long MessagesProcessedCount { get; private set; }

        /// <inheritdoc/>
        public double AvgNotificationsPerMessage { get; private set; }

        /// <inheritdoc/>
        public double AvgMessageSize { get; private set; }

        /// <inheritdoc/>
        public double MaxMessageSplitRatio { get; private set; }

        /// <summary>
        /// Create instance of NetworkMessageEncoder.
        /// </summary>
        /// <param name="config"> injected configuration. </param>
        /// <param name="metrics"> Metrics context </param>
        /// <param name="logger"> Logger to be used for reporting. </param>
        public NetworkMessageEncoder(IEngineConfiguration config,
            IMetricsContext metrics, ILogger<NetworkMessageEncoder> logger)
            : this(metrics ?? throw new ArgumentNullException(nameof(metrics)))
        {
            _logger = logger;
            _config = config;
        }

        /// <inheritdoc/>
        public IEnumerable<IEvent> Encode(Func<IEvent> factory,
            IEnumerable<SubscriptionNotificationModel> notifications, int maxMessageSize, bool asBatch)
        {
            //
            // by design all messages are generated in the same session context, therefore it is safe to
            // get the first message's context
            //
            var encodingContext = notifications.FirstOrDefault(m => m.ServiceMessageContext != null)?.ServiceMessageContext;
            var chunkedMessages = new List<IEvent>();
            if (encodingContext == null)
            {
                // Drop all messages
                Drop(notifications);
                return chunkedMessages;
            }

            var networkMessages = GetNetworkMessages(notifications, asBatch);
            foreach (var (notificationsPerMessage, networkMessage, output, retain, ttl) in networkMessages)
            {
                var chunks = networkMessage.Encode(encodingContext, maxMessageSize);
                var notificationsPerChunk = notificationsPerMessage / (double)chunks.Count;
                var validChunks = 0;
                foreach (var body in chunks)
                {
                    if (body.Length == 0)
                    {
                        //
                        // Failed to press a notification into message size limit
                        // This is somewhat correct as the smallest dropped chunk is
                        // a message containing only a single data set message which
                        // contains (parts) of a notification.
                        //
                        _logger.LogWarning("Resulting chunk is too large, dropped a notification.");
                        continue;
                    }
                    validChunks++;
                    AvgMessageSize = ((AvgMessageSize * MessagesProcessedCount) + body.Length) /
                        (MessagesProcessedCount + 1);
                    AvgNotificationsPerMessage = ((AvgNotificationsPerMessage * MessagesProcessedCount) +
                        notificationsPerChunk) / (MessagesProcessedCount + 1);
                    MessagesProcessedCount++;
                }

                if (validChunks > 0)
                {
                    var chunkedMessage = factory()
                        .AddProperty(OpcUa.Constants.MessagePropertySchemaKey, networkMessage.MessageSchema)
                        .AddProperty(OpcUa.Constants.MessagePropertyRoutingKey, networkMessage.DataSetWriterGroup)
                        .SetTimestamp(DateTime.UtcNow)
                        .SetContentEncoding(networkMessage.ContentEncoding)
                        .SetContentType(networkMessage.ContentType)
                        .SetTopic(output)
                        .SetRetain(retain)
                        .SetTtl(ttl)
                        .AddBuffers(chunks);
                    chunkedMessages.Add(chunkedMessage);
                }

                // We dropped a number of notifications but processed the remainder successfully
                var tooBig = chunks.Count - validChunks;
                NotificationsDroppedCount += tooBig;
                if (notificationsPerMessage > tooBig)
                {
                    NotificationsProcessedCount += notificationsPerMessage - tooBig;
                }

                //
                // how many notifications per message resulted in how many buffers. We track the
                // split size to provide users with an indication how many times chunks had to
                // be created so they can configure publisher to improve performance.
                //
                if (notificationsPerMessage > 0 && notificationsPerMessage < validChunks)
                {
                    var splitSize = validChunks / notificationsPerMessage;
                    if (splitSize > MaxMessageSplitRatio)
                    {
                        MaxMessageSplitRatio = splitSize;
                    }
                }
            }
            return chunkedMessages;
        }

        /// <summary>
        /// Produce network messages from the data set message model
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="isBatched"></param>
        /// <returns></returns>
        private List<(int, PubSubMessage, string, bool, TimeSpan)> GetNetworkMessages(IEnumerable<SubscriptionNotificationModel> messages,
            bool isBatched)
        {
            var standardsCompliant = _config.UseStandardsCompliantEncoding;
            var result = new List<(int, PubSubMessage, string, bool, TimeSpan)>();
            // Group messages by publisher, then writer group and then by dataset class id
            foreach (var publishers in messages
                .Select(m => (Notification: m, Context: m.Context as WriterGroupMessageContext))
                .Where(m => m.Context != null)
                .GroupBy(m => m.Context.PublisherId))
            {
                var publisherId = publishers.Key;
                foreach (var groups in publishers.GroupBy(m => m.Context.WriterGroup))
                {
                    var writerGroup = groups.Key;
                    if (writerGroup?.MessageSettings == null)
                    {
                        // Must have a writer group
                        Drop(groups.Select(m => m.Notification));
                        continue;
                    }
                    var encoding = writerGroup.MessageType ?? MessageEncoding.Json;
                    var messageMask = writerGroup.MessageSettings.NetworkMessageContentMask;
                    var hasSamplesPayload = (messageMask & NetworkMessageContentMask.MonitoredItemMessage) != 0;
                    if (hasSamplesPayload && !isBatched)
                    {
                        messageMask |= NetworkMessageContentMask.SingleDataSetMessage;
                    }
                    var networkMessageContentMask = messageMask.ToStackType(encoding);
                    foreach (var dataSetClass in groups
                        .GroupBy(m => m.Context.Writer?.DataSet?.DataSetMetaData?.DataSetClassId ?? Guid.Empty))
                    {
                        var dataSetClassId = dataSetClass.Key;
                        var currentMessage = CreateMessage(writerGroup, encoding,
                            networkMessageContentMask, dataSetClassId, publisherId);
                        var currentNotificationCount = 0;
                        foreach (var message in dataSetClass)
                        {
                            if (message.Context.Writer == null ||
                                (hasSamplesPayload && !encoding.HasFlag(MessageEncoding.Json)))
                            {
                                // Must have a writer or if samples mode, must be json
                                Drop(message.Notification.YieldReturn());
                                continue;
                            }
                            var dataSetMessageContentMask =
                                (message.Context.Writer.MessageSettings?.DataSetMessageContentMask).ToStackType(
                                    message.Context.Writer.DataSetFieldContentMask, encoding);
                            var dataSetFieldContentMask =
                                    message.Context.Writer.DataSetFieldContentMask.ToStackType();

                            if (message.Notification.MessageType != MessageType.Metadata)
                            {
                                Debug.Assert(message.Notification.Notifications != null);
                                var notificationQueues = message.Notification.Notifications
                                    .GroupBy(m => m.DataSetFieldName)
                                    .Select(c => new Queue<MonitoredItemNotificationModel>(c.ToArray()))
                                    .ToArray();

                                while (notificationQueues.Any(q => q.Count > 0))
                                {
                                    var orderedNotifications = notificationQueues
                                        .Select(q => q.Count > 0 ? q.Dequeue() : null)
                                        .Where(s => s != null);

                                    if (!hasSamplesPayload)
                                    {
                                        // Create regular data set messages
                                        BaseDataSetMessage dataSetMessage = encoding.HasFlag(MessageEncoding.Json)
                                            ? new JsonDataSetMessage
                                            {
                                                UseCompatibilityMode = !standardsCompliant,
                                                DataSetWriterName = message.Context.Writer.DataSetWriterName
                                            }
                                            : new UadpDataSetMessage();

                                        dataSetMessage.DataSetWriterId = message.Notification.SubscriptionId;
                                        dataSetMessage.MessageType = message.Notification.MessageType;
                                        dataSetMessage.MetaDataVersion = message.Notification.MetaData?.ConfigurationVersion
                                            ?? kEmptyConfiguration;
                                        dataSetMessage.DataSetMessageContentMask = dataSetMessageContentMask;
                                        dataSetMessage.Timestamp = message.Notification.Timestamp;
                                        dataSetMessage.SequenceNumber = message.Context.SequenceNumber;
                                        dataSetMessage.Payload = new DataSet(orderedNotifications.ToDictionary(
                                            s => s.DataSetFieldName, s => s.Value), (uint)dataSetFieldContentMask);

                                        AddMessage(dataSetMessage);
                                    }
                                    else
                                    {
                                        // Add monitored item message payload to network message to handle backcompat
                                        foreach (var itemNotifications in orderedNotifications.GroupBy(f => f.Id + f.MessageId))
                                        {
                                            var notificationsInGroup = itemNotifications.ToList();
                                            Debug.Assert(notificationsInGroup.Count != 0);
                                            //
                                            // Special monitored item handling for events and conditions. Collate all
                                            // values into a single key value data dictionary extension object value.
                                            // Regular notifications we send as single messages.
                                            //
                                            if (notificationsInGroup.Count > 1 &&
                                                (message.Notification.MessageType == MessageType.Event ||
                                                 message.Notification.MessageType == MessageType.Condition))
                                            {
                                                Debug.Assert(notificationsInGroup
                                                    .Select(n => n.DataSetFieldName).Distinct().Count() == notificationsInGroup.Count,
                                                    "There should not be duplicates in fields in a group.");
                                                Debug.Assert(notificationsInGroup
                                                    .All(n => n.SequenceNumber == notificationsInGroup[0].SequenceNumber),
                                                    "All notifications in the group should have the same sequence number.");

                                                var eventNotification = notificationsInGroup[0]; // No clone, mutate ok.
                                                eventNotification.Value = new DataValue
                                                {
                                                    Value = new EncodeableDictionary(notificationsInGroup
                                                        .Select(n => new KeyDataValuePair(n.DataSetFieldName, n.Value)))
                                                };
                                                eventNotification.DataSetFieldName = notificationsInGroup[0].DisplayName;
                                                notificationsInGroup = new List<MonitoredItemNotificationModel> {
                                                    eventNotification
                                                };
                                            }
                                            foreach (var notification in notificationsInGroup)
                                            {
                                                var dataSetMessage = new MonitoredItemMessage
                                                {
                                                    UseCompatibilityMode = !standardsCompliant,
                                                    ApplicationUri = message.Notification.ApplicationUri,
                                                    EndpointUrl = message.Notification.EndpointUrl,
                                                    NodeId = notification.NodeId,
                                                    MessageType = message.Notification.MessageType,
                                                    DataSetMessageContentMask = dataSetMessageContentMask,
                                                    Timestamp = message.Notification.Timestamp,
                                                    SequenceNumber = message.Context.SequenceNumber,
                                                    ExtensionFields = message.Context.Writer.DataSet.ExtensionFields,
                                                    Payload = new DataSet(notification.DataSetFieldName, notification.Value,
                                                        (uint)dataSetFieldContentMask)
                                                };
                                                AddMessage(dataSetMessage);
                                            }
                                        }
                                    }

                                    //
                                    // Add message and number of notifications processed count to method result.
                                    // Checks current length and splits if max items reached if configured.
                                    //
                                    void AddMessage(BaseDataSetMessage dataSetMessage)
                                    {
                                        currentMessage.Messages.Add(dataSetMessage);
                                        var maxMessagesToPublish = writerGroup.MessageSettings?.MaxMessagesPerPublish ??
                                            _config.DefaultMaxMessagesPerPublish;
                                        if (maxMessagesToPublish != null && currentMessage.Messages.Count >= maxMessagesToPublish)
                                        {
                                            result.Add((currentNotificationCount, currentMessage, default, false, default));
                                            currentMessage = CreateMessage(writerGroup, encoding, networkMessageContentMask,
                                                dataSetClassId, publisherId);
                                            currentNotificationCount = 0;
                                        }
                                    }
                                }
                                currentNotificationCount++;
                            }
                            else if (message.Notification.MetaData != null && !hasSamplesPayload)
                            {
                                if (currentMessage.Messages.Count > 0)
                                {
                                    // Start a new message but first emit current
                                    result.Add((currentNotificationCount, currentMessage, default, false, default));
                                    currentMessage = CreateMessage(writerGroup, encoding, networkMessageContentMask,
                                        dataSetClassId, publisherId);
                                    currentNotificationCount = 0;
                                }
                                PubSubMessage metadataMessage = encoding.HasFlag(MessageEncoding.Json)
                                    ? new JsonMetaDataMessage
                                    {
                                        UseAdvancedEncoding = !standardsCompliant,
                                        UseGzipCompression = encoding.HasFlag(MessageEncoding.IsGzipCompressed),
                                        DataSetWriterId = message.Notification.SubscriptionId,
                                        MetaData = message.Notification.MetaData,
                                        MessageId = Guid.NewGuid().ToString(),
                                        DataSetWriterName = message.Context.Writer.DataSetWriterName
                                    } : new UadpMetaDataMessage
                                    {
                                        DataSetWriterId = message.Notification.SubscriptionId,
                                        MetaData = message.Notification.MetaData
                                    };
                                metadataMessage.PublisherId = publisherId;
                                metadataMessage.DataSetWriterGroup = writerGroup.WriterGroupId;

                                var queueName = message.Context.Writer.MetaDataQueueName
                                    ?? _config.DefaultMetaDataQueueName;
                                result.Add((0, metadataMessage, queueName, true,
                                    message.Context.Writer.MetaDataUpdateTime ?? default));
                            }
                        }
                        if (currentMessage.Messages.Count > 0)
                        {
                            result.Add((currentNotificationCount, currentMessage, default, false, default));
                        }

                        BaseNetworkMessage CreateMessage(WriterGroupModel writerGroup, MessageEncoding encoding,
                            uint networkMessageContentMask, Guid dataSetClassId, string publisherId)
                        {
                            BaseNetworkMessage currentMessage = encoding.HasFlag(MessageEncoding.Json) ?
                                new JsonNetworkMessage
                                {
                                    UseAdvancedEncoding = !standardsCompliant,
                                    UseGzipCompression = encoding.HasFlag(MessageEncoding.IsGzipCompressed),
                                    UseArrayEnvelope = !standardsCompliant && isBatched,
                                    MessageId = () => Guid.NewGuid().ToString()
                                } : new UadpNetworkMessage
                                {
                                    //   WriterGroupId = writerGroup.Index,
                                    //   GroupVersion = writerGroup.Version,
                                    SequenceNumber = () => SequenceNumber.Increment16(ref _sequenceNumber),
                                    Timestamp = DateTime.UtcNow,
                                    PicoSeconds = 0
                                };
                            currentMessage.NetworkMessageContentMask = networkMessageContentMask;
                            currentMessage.PublisherId = publisherId;
                            currentMessage.DataSetClassId = dataSetClassId;
                            currentMessage.DataSetWriterGroup = writerGroup.WriterGroupId;
                            return currentMessage;
                        }
                    }
                }
            }
            return result;
        }

        private void Drop(IEnumerable<SubscriptionNotificationModel> messages)
        {
            var totalNotifications = messages.Sum(m => m?.Notifications?.Count ?? 0);
            NotificationsDroppedCount += totalNotifications;
            _logger.LogWarning("Dropped {TotalNotifications} values", totalNotifications);
        }

        private static readonly ConfigurationVersionDataType kEmptyConfiguration =
            new() { MajorVersion = 1u };
        private readonly IEngineConfiguration _config;
        private readonly ILogger _logger;
        private uint _sequenceNumber;

        /// <summary>
        /// Create observable metric registrations
        /// </summary>
        /// <param name="metrics"></param>
        private NetworkMessageEncoder(IMetricsContext metrics)
        {
            Diagnostics.Meter.CreateObservableCounter("iiot_edge_publisher_encoded_notifications",
                () => new Measurement<long>(NotificationsProcessedCount, metrics.TagList), "Notifications",
                "Number of successfully processed subscription notifications received from OPC client.");
            Diagnostics.Meter.CreateObservableCounter("iiot_edge_publisher_dropped_notifications",
                () => new Measurement<long>(NotificationsDroppedCount, metrics.TagList), "Notifications",
                "Number of incoming subscription notifications that are too big to be processed based " +
                "on the message size limits or other issues with the notification.");
            Diagnostics.Meter.CreateObservableCounter("iiot_edge_publisher_processed_messages",
                () => new Measurement<long>(MessagesProcessedCount, metrics.TagList), "Messages",
                "Number of successfully generated messages that are to be sent using the message sender");
            Diagnostics.Meter.CreateObservableGauge("iiot_edge_publisher_notifications_per_message_average",
                () => new Measurement<double>(AvgNotificationsPerMessage, metrics.TagList), "Notifications/Message",
                "Average subscription notifications packed into a message");
            Diagnostics.Meter.CreateObservableGauge("iiot_edge_publisher_encoded_message_size_average",
                () => new Measurement<double>(AvgMessageSize, metrics.TagList), "Bytes",
                "Average size of a message through the lifetime of the encoder.");
            Diagnostics.Meter.CreateObservableGauge("iiot_edge_publisher_chunk_size_average",
                () => new Measurement<double>(AvgMessageSize / (4 * 1024), metrics.TagList), "4kb Chunks",
                "IoT Hub chunk size average");
            Diagnostics.Meter.CreateObservableGauge("iiot_edge_publisher_message_split_ratio_max",
                () => new Measurement<double>(MaxMessageSplitRatio, metrics.TagList), "Splits",
                "The message split ration specifies into how many messages a subscription notification had to be split. " +
                "Less is better for performance. If the number is large user should attempt to limit the number of " +
                "notifications in a message using configuration.");
        }
    }
}

﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Azure.IIoT.OpcUa.Publisher {
    using Azure.IIoT.OpcUa.Shared.Models;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.IIoT.OpcUa.Publisher.Stack.Models;

    /// <summary>
    /// Writer group
    /// </summary>
    public interface IMessageSource : IAsyncDisposable {

        /// <summary>
        /// Subscribe to writer messages
        /// </summary>
        event EventHandler<SubscriptionNotificationModel> OnMessage;

        /// <summary>
        /// Called when ValueChangesCount or DataChangesCount are resetted
        /// </summary>
        event EventHandler<EventArgs> OnCounterReset;

        /// <summary>
        /// Start trigger
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        ValueTask StartAsync(CancellationToken ct);

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="config"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        ValueTask UpdateAsync(WriterGroupJobModel config,
            CancellationToken ct);
    }
}

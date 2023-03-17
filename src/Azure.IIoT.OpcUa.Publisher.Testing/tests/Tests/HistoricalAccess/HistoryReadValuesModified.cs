// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------
#nullable enable
namespace Azure.IIoT.OpcUa.Publisher.Testing.Tests
{
    using Azure.IIoT.OpcUa.Publisher.Models;
    using Azure.IIoT.OpcUa.Publisher.Testing.Fixtures;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class HistoryReadValuesModifiedTests<T>
    {
        /// <summary>
        /// Create history services tests
        /// </summary>
        /// <param name="services"></param>
        /// <param name="connection"></param>
        public HistoryReadValuesModifiedTests(BaseServerFixture server,
            Func<IHistoryServices<T>> services, T connection)
        {
            _server = server;
            _services = services;
            _connection = connection;
        }

        public async Task HistoryReadInt16ValuesModifiedTestAsync()
        {
            var services = _services();
            const string samples = "s=1:Azure.IIoT.OpcUa.Publisher.Testing.Servers.HistoricalAccess.Data.Sample.Int16.txt";

            var results = await services.HistoryReadModifiedValuesAsync(_connection,
                new HistoryReadRequestModel<ReadModifiedValuesDetailsModel>
                {
                    NodeId = "http://opcfoundation.org/HistoricalAccess#" + samples,
                    Details = new ReadModifiedValuesDetailsModel
                    {
                        StartTime = _server.Now - TimeSpan.FromDays(600),
                        EndTime = _server.Now + TimeSpan.FromDays(1),
                        NumValues = 14
                    }
                }).ConfigureAwait(false);

            Assert.NotNull(results.History);
            Assert.Equal(0, results.History.Length);
            Assert.Empty(results.History);
        }

        public async Task HistoryStreamInt16ValuesModifiedTestAsync()
        {
            var services = _services();
            const string samples = "s=1:Azure.IIoT.OpcUa.Publisher.Testing.Servers.HistoricalAccess.Data.Sample.Int16.txt";

            var history = await services.HistoryStreamModifiedValuesAsync(_connection,
                new HistoryReadRequestModel<ReadModifiedValuesDetailsModel>
                {
                    NodeId = "http://opcfoundation.org/HistoricalAccess#" + samples,
                    Details = new ReadModifiedValuesDetailsModel
                    {
                        EndTime = _server.Now + TimeSpan.FromDays(1),
                        NumValues = 10
                    }
                }).ToListAsync().ConfigureAwait(false);

            Assert.NotNull(history);
            Assert.Equal(0, history.Count);
        }

        private readonly T _connection;
        private readonly BaseServerFixture _server;
        private readonly Func<IHistoryServices<T>> _services;
    }
}

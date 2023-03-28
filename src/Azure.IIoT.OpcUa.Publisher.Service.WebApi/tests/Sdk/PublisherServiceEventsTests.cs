// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Azure.IIoT.OpcUa.Publisher.Service.WebApi.Tests.Sdk.SignalR
{
    using Azure.IIoT.OpcUa.Publisher.Service.Sdk;
    using Azure.IIoT.OpcUa.Publisher.Models;
    using Autofac;
    using Furly.Extensions.Serializers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;
    using Divergic.Logging.Xunit;
    using xRetry;

    public sealed class PublisherServiceEventsTests : IDisposable
    {
        public PublisherServiceEventsTests(ITestOutputHelper output)
        {
            _factory = WebAppFixture.Create(LogFactory.Create(output));
            _output = output;
        }

        public void Dispose()
        {
            _factory.Dispose();
        }

        private const int kTimeoutMillis = 10000;
        private readonly WebAppFixture _factory;
        private readonly ITestOutputHelper _output;

        [RetryTheory(1)]
        [MemberData(nameof(GetScalarValues))]
        public async Task TestPublishTelemetryEventAndReceiveAsync(VariantValue v)
        {
            using var scope = _factory.CreateClientScope(_output, TestSerializerType.NewtonsoftJson);
            var bus = _factory.Resolve<ISubscriberMessageProcessor>();
            var client = scope.Resolve<IPublisherServiceEvents>();

            const string endpointId = "testid";

            var result = new TaskCompletionSource<MonitoredItemMessageModel>(TaskCreationOptions.RunContinuationsAsynchronously);
            await using (await client.NodePublishSubscribeByEndpointAsync(endpointId, ev =>
            {
                result.SetResult(ev);
                return Task.CompletedTask;
            }).ConfigureAwait(false))
            {
                var expected = new MonitoredItemMessageModel
                {
                    DataSetWriterId = "testid",
                    EndpointId = endpointId,
                    DisplayName = "holla",
                    NodeId = "nodeid",
                    SourceTimestamp = DateTime.UtcNow,
                    Timestamp = DateTime.UtcNow,
                    Value = v
                };
                await bus.HandleSampleAsync(expected).ConfigureAwait(false);
                await Task.WhenAny(result.Task, Task.Delay(kTimeoutMillis)).ConfigureAwait(false);

                Assert.True(result.Task.IsCompleted, $"Timed out with value: {v.ToJson()}");
                var received = result.Task.Result;

                Assert.Equal(expected.DisplayName, received.DisplayName);
                Assert.Equal(expected.DataSetWriterId, received.DataSetWriterId);
                Assert.Equal(expected.NodeId, received.NodeId);
                Assert.Equal(expected.SourceTimestamp, received.SourceTimestamp);
                Assert.Equal(expected.Timestamp, received.Timestamp);

                Assert.NotNull(received?.Value);
                Assert.Equal(expected.Value, received.Value);
            }
        }

        [RetryTheory(1)]
        [InlineData(10)]
        [InlineData(4455)]
        [InlineData(262345)]
        public async Task TestPublishPublisherEventAndReceiveMultipleAsync(int total)
        {
            using var scope = _factory.CreateClientScope(_output, TestSerializerType.NewtonsoftJson);
            var bus = _factory.Resolve<ISubscriberMessageProcessor>();
            var client = scope.Resolve<IPublisherServiceEvents>();

            const string endpointId = "testid";
            var expected = new MonitoredItemMessageModel
            {
                DataSetWriterId = "testid",
                EndpointId = endpointId,
                DisplayName = "holla",
                NodeId = "nodeid",
                SourceTimestamp = DateTime.UtcNow,
                Timestamp = DateTime.UtcNow,
                Value = 234234
            };
            var result = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var counter = 0;
            await using (await client.NodePublishSubscribeByEndpointAsync(endpointId, ev =>
            {
                counter++;
                if (counter == total)
                {
                    result.SetResult(true);
                }
                return Task.CompletedTask;
            }).ConfigureAwait(false))
            {
                for (var i = 0; i < total; i++)
                {
                    await bus.HandleSampleAsync(expected).ConfigureAwait(false);
                }

                await Task.WhenAny(result.Task, Task.Delay(kTimeoutMillis)).ConfigureAwait(false);
                Assert.True(result.Task.IsCompleted, $"{counter} received instead of {total}");
            }
        }

        public static IEnumerable<(VariantValue, object)> GetStrings()
        {
            yield return ("", "");
            yield return ("str ing", "str ing");
            yield return ("{}", "{}");
            yield return (Array.Empty<byte>(), Array.Empty<byte>());
            yield return (new byte[1000], new byte[1000]);
            yield return (new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
            yield return (Encoding.UTF8.GetBytes("utf-8-string"), Encoding.UTF8.GetBytes("utf-8-string"));
        }

        public static IEnumerable<(VariantValue, object)> GetValues()
        {
#if FALSE
            yield return ((long?)null, (long?)null);
            yield return ((ulong?)null, (ulong?)null);
            yield return ((short?)null, (short?)null);
            yield return ((int?)null, (int?)null);
            yield return ((bool?)null, (bool?)null);
            yield return ((sbyte?)null, (sbyte?)null);
            yield return ((byte?)null, (byte?)null);
            yield return ((uint?)null, (uint?)null);
            yield return ((ushort?)null, (ushort?)null);
            yield return ((decimal?)null, (decimal?)null);
            yield return ((double?)null, (double?)null);
            yield return ((float?)null, (float?)null);
            yield return ((DateTime?)null, (DateTime?)null);
            yield return ((DateTimeOffset?)null, (DateTimeOffset?)null);
            yield return ((TimeSpan?)null, (TimeSpan?)null);
#endif
            yield return (true, true);
            yield return (false, false);
            yield return ((sbyte)1, (sbyte)1);
            yield return ((sbyte)-1, (sbyte)-1);
            yield return ((sbyte)0, (sbyte)0);
            yield return (sbyte.MaxValue, sbyte.MaxValue);
            yield return (sbyte.MinValue, sbyte.MinValue);
            yield return ((short)1, (short)1);
            yield return ((short)-1, (short)-1);
            yield return ((short)0, (short)0);
            yield return (short.MaxValue, short.MaxValue);
            yield return (short.MinValue, short.MinValue);
            yield return (1, 1);
            yield return (-1, -1);
            yield return (0, 0);
            yield return (int.MaxValue, int.MaxValue);
            yield return (int.MinValue, int.MinValue);
            yield return (1L, 1L);
            yield return (-1L, -1L);
            yield return (0L, 0L);
            yield return (long.MaxValue, long.MaxValue);
            yield return (long.MinValue, long.MinValue);
            yield return (1UL, 1UL);
            yield return (0UL, 0UL);
            yield return (ulong.MaxValue, ulong.MaxValue);
            yield return (1u, 1u);
            yield return (0u, 0u);
            yield return (uint.MaxValue, uint.MaxValue);
            yield return ((ushort)1, (ushort)1);
            yield return ((ushort)0, (ushort)0);
            yield return (ushort.MaxValue, ushort.MaxValue);
            yield return ((byte)1, (byte)1);
            yield return ((byte)0, (byte)0);
            yield return (1.0, 1.0);
            yield return (-1.0, -1.0);
            yield return (0.0, 0.0);
            yield return (byte.MaxValue, byte.MaxValue);
            yield return (double.MaxValue, double.MaxValue);
            yield return (double.MinValue, double.MinValue);
            yield return (double.PositiveInfinity, double.PositiveInfinity);
            yield return (double.NegativeInfinity, double.NegativeInfinity);
            yield return (1.0f, 1.0f);
            yield return (-1.0f, -1.0f);
            yield return (0.0f, 0.0f);
            yield return (float.MaxValue, float.MaxValue);
            yield return (float.MinValue, float.MinValue);
            yield return (float.PositiveInfinity, float.PositiveInfinity);
            yield return (float.NegativeInfinity, float.NegativeInfinity);
            yield return ((decimal)1.0, (decimal)1.0);
            yield return ((decimal)-1.0, (decimal)-1.0);
            yield return ((decimal)0.0, (decimal)0.0);
            yield return ((decimal)1234567, (decimal)1234567);
            yield return (kGuid, kGuid);
            yield return (Guid.Empty, Guid.Empty);
            yield return (kNow1, kNow1);
            yield return (DateTime.MaxValue, DateTime.MaxValue);
            yield return (DateTime.MinValue, DateTime.MinValue);
            yield return (kNow2, kNow2);
            yield return (TimeSpan.Zero, TimeSpan.Zero);
            yield return (TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
            yield return (TimeSpan.FromDays(5555), TimeSpan.FromDays(5555));
            yield return (TimeSpan.MaxValue, TimeSpan.MaxValue);
            yield return (TimeSpan.MinValue, TimeSpan.MinValue);
        }

        private static readonly Guid kGuid = Guid.NewGuid();
        private static readonly DateTime kNow1 = DateTime.UtcNow;
        private static readonly DateTimeOffset kNow2 = DateTimeOffset.UtcNow;

        public static IEnumerable<object[]> GetScalarValues()
        {
            return GetStrings()
                .Select(v => new object[] { v.Item1 })
                .Concat(GetValues()
                .Select(v => new object[] { v.Item2 }));
        }
    }
}

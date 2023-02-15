// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.OpcUa.Publisher.Tests.Api.Json {
    using Microsoft.Azure.IIoT.Services.OpcUa.Publisher.Tests.Api;
    using Microsoft.Azure.IIoT.Services.OpcUa.Publisher.Tests;
    using Microsoft.Azure.IIoT.Api.Publisher.Adapter;
    using Microsoft.Azure.IIoT.Http.Default;
    using Microsoft.Azure.IIoT.OpcUa.Api.Publisher.Clients;
    using Microsoft.Azure.IIoT.OpcUa.Core.Models;
    using Microsoft.Azure.IIoT.OpcUa.Testing.Fixtures;
    using Microsoft.Azure.IIoT.OpcUa.Testing.Tests;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Utils;
    using Opc.Ua;
    using Serilog;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Xunit;

    [Collection(ReadJsonCollection.Name)]
    public class BrowseControllerTest : IClassFixture<WebAppFixture> {

        public BrowseControllerTest(WebAppFixture factory, TestServerFixture server) {
            _factory = factory;
            _server = server;
            _hostEntry = Try.Op(() => Dns.GetHostEntry(Utils.GetHostName()))
                ?? Try.Op(() => Dns.GetHostEntry("localhost"));
        }

        private BrowseServicesTests<string> GetTests() {
            var client = _factory.CreateClient(); // Call to create server
            var module = _factory.Resolve<ITestModule>();
            module.Connection = Connection;
            var log = _factory.Resolve<ILogger>();
            var serializer = _factory.Resolve<IJsonSerializer>();
            return new BrowseServicesTests<string>(() => // Create an adapter over the api
                new TwinServicesApiAdapter(
                    new TwinServiceClient(new HttpClient(_factory, log),
                    new TestConfig(client.BaseAddress), serializer)), "fakeid");
        }

        public ConnectionModel Connection => new() {
            Endpoint = new EndpointModel {
                Url = $"opc.tcp://{_hostEntry?.HostName ?? "localhost"}:{_server.Port}/UA/SampleServer",
                AlternativeUrls = _hostEntry?.AddressList
                .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .Select(ip => $"opc.tcp://{ip}:{_server.Port}/UA/SampleServer").ToHashSet(),
                Certificate = _server.Certificate?.RawData?.ToThumbprint()
            }
        };

        private readonly WebAppFixture _factory;
        private readonly TestServerFixture _server;
        private readonly IPHostEntry _hostEntry;

        [Fact]
        public async Task NodeBrowseInRootTest1Async() {
            await GetTests().NodeBrowseInRootTest1Async();
        }

        [Fact]
        public async Task NodeBrowseInRootTest2Async() {
            await GetTests().NodeBrowseInRootTest2Async();
        }

        [Fact]
        public async Task NodeBrowseFirstInRootTest1Async() {
            await GetTests().NodeBrowseFirstInRootTest1Async();
        }

        [Fact]
        public async Task NodeBrowseFirstInRootTest2Async() {
            await GetTests().NodeBrowseFirstInRootTest2Async();
        }

        [Fact]
        public async Task NodeBrowseBoilersObjectsTest1Async() {
            await GetTests().NodeBrowseBoilersObjectsTest1Async();
        }

        [Fact]
        public async Task NodeBrowseBoilersObjectsTest2Async() {
            await GetTests().NodeBrowseBoilersObjectsTest2Async();
        }

        [Fact]
        public async Task NodeBrowseDataAccessObjectsTest1Async() {
            await GetTests().NodeBrowseDataAccessObjectsTest1Async();
        }

        [Fact]
        public async Task NodeBrowseDataAccessObjectsTest2Async() {
            await GetTests().NodeBrowseDataAccessObjectsTest2Async();
        }

        [Fact]
        public async Task NodeBrowseDataAccessObjectsTest3Async() {
            await GetTests().NodeBrowseDataAccessObjectsTest3Async();
        }

        [Fact]
        public async Task NodeBrowseDataAccessObjectsTest4Async() {
            await GetTests().NodeBrowseDataAccessObjectsTest4Async();
        }

        [Fact]
        public async Task NodeBrowseDataAccessFC1001Test1Async() {
            await GetTests().NodeBrowseDataAccessFC1001Test1Async();
        }

        [Fact]
        public async Task NodeBrowseDataAccessFC1001Test2Async() {
            await GetTests().NodeBrowseDataAccessFC1001Test2Async();
        }

        [Fact]
        public async Task NodeBrowseStaticScalarVariablesTestAsync() {
            await GetTests().NodeBrowseStaticScalarVariablesTestAsync();
        }

        [Fact]
        public async Task NodeBrowseStaticArrayVariablesTestAsync() {
            await GetTests().NodeBrowseStaticArrayVariablesTestAsync();
        }

        [Fact]
        public async Task NodeBrowseStaticScalarVariablesTestWithFilter1Async() {
            await GetTests().NodeBrowseStaticScalarVariablesTestWithFilter1Async();
        }

        [Fact]
        public async Task NodeBrowseStaticScalarVariablesTestWithFilter2Async() {
            await GetTests().NodeBrowseStaticScalarVariablesTestWithFilter2Async();
        }

        [Fact]
        public async Task NodeBrowseStaticArrayVariablesWithValuesTestAsync() {
            await GetTests().NodeBrowseStaticArrayVariablesWithValuesTestAsync();
        }

        [SkippableFact]
        public async Task NodeBrowseStaticArrayVariablesRawModeTestAsync() {
            Skip.If(true, "No API impl.");
            await GetTests().NodeBrowseStaticArrayVariablesRawModeTestAsync();
        }

        [Fact]
        public async Task NodeBrowseContinuationTest1Async() {
            await GetTests().NodeBrowseContinuationTest1Async();
        }

        [Fact]
        public async Task NodeBrowseContinuationTest2Async() {
            await GetTests().NodeBrowseContinuationTest2Async();
        }

        [Fact]
        public async Task NodeBrowseContinuationTest3Async() {
            await GetTests().NodeBrowseContinuationTest3Async();
        }

        [Fact]
        public async Task NodeBrowsePathStaticScalarMethod3Test1Async() {
            await GetTests().NodeBrowsePathStaticScalarMethod3Test1Async();
        }

        [Fact]
        public async Task NodeBrowsePathStaticScalarMethod3Test2Async() {
            await GetTests().NodeBrowsePathStaticScalarMethod3Test2Async();
        }

        [Fact]
        public async Task NodeBrowsePathStaticScalarMethod3Test3Async() {
            await GetTests().NodeBrowsePathStaticScalarMethod3Test3Async();
        }

        [Fact]
        public async Task NodeBrowsePathStaticScalarMethodsTestAsync() {
            await GetTests().NodeBrowsePathStaticScalarMethodsTestAsync();
        }

        [Fact]
        public async Task NodeBrowseDiagnosticsNoneTestAsync() {
            await GetTests().NodeBrowseDiagnosticsNoneTestAsync();
        }

        [Fact]
        public async Task NodeBrowseDiagnosticsStatusTestAsync() {
            await GetTests().NodeBrowseDiagnosticsStatusTestAsync();
        }

        [Fact]
        public async Task NodeBrowseDiagnosticsOperationsTestAsync() {
            await GetTests().NodeBrowseDiagnosticsOperationsTestAsync();
        }

        [Fact]
        public async Task NodeBrowseDiagnosticsVerboseTestAsync() {
            await GetTests().NodeBrowseDiagnosticsVerboseTestAsync();
        }
    }
}

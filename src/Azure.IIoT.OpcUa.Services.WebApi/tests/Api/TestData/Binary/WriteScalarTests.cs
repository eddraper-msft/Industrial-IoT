// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Azure.IIoT.OpcUa.Services.WebApi.Api.TestData.Binary
{
    using Azure.IIoT.OpcUa.Services.WebApi.Clients;
    using Azure.IIoT.OpcUa.Publisher.Stack;
    using Azure.IIoT.OpcUa.Services.Sdk.Clients;
    using Azure.IIoT.OpcUa.Services.WebApi;
    using Azure.IIoT.OpcUa.Testing.Fixtures;
    using Azure.IIoT.OpcUa.Testing.Tests;
    using Furly.Extensions.Serializers;
    using System.Threading.Tasks;
    using Xunit;

    [Collection(WriteCollection.Name)]
    public class WriteScalarTests : IClassFixture<WebAppFixture>
    {
        public WriteScalarTests(WebAppFixture factory, TestDataServer server)
        {
            _factory = factory;
            _server = server;
        }

        private WriteScalarValueTests<string> GetTests()
        {
            var client = _factory.CreateClient(); // Call to create server
            var registry = _factory.Resolve<IEndpointManager>();
            var endpointId = registry.RegisterEndpointAsync(_server.GetConnection().Endpoint).Result;
            var serializer = _factory.Resolve<IBinarySerializer>();
            return new WriteScalarValueTests<string>(() => // Create an adapter over the api
                new TwinWebApiAdapter(
                    new TwinServiceClient(_factory,
                    new TestConfig(client.BaseAddress), serializer)),
                        endpointId, (ep, n, s) => _server.Client.ReadValueAsync(_server.GetConnection(), n, s));
        }

        private readonly WebAppFixture _factory;
        private readonly TestDataServer _server;

        [Fact]
        public Task NodeWriteStaticScalarBooleanValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarBooleanValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarBooleanValueVariableWithBrowsePathTest1Async()
        {
            return GetTests().NodeWriteStaticScalarBooleanValueVariableWithBrowsePathTest1Async();
        }

        [Fact]
        public Task NodeWriteStaticScalarBooleanValueVariableWithBrowsePathTest2Async()
        {
            return GetTests().NodeWriteStaticScalarBooleanValueVariableWithBrowsePathTest2Async();
        }

        [Fact]
        public Task NodeWriteStaticScalarBooleanValueVariableWithBrowsePathTest3Async()
        {
            return GetTests().NodeWriteStaticScalarBooleanValueVariableWithBrowsePathTest3Async();
        }

        [Fact]
        public Task NodeWriteStaticScalarSByteValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarSByteValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarByteValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarByteValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarInt16ValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarInt16ValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarUInt16ValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarUInt16ValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarInt32ValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarInt32ValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarUInt32ValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarUInt32ValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarInt64ValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarInt64ValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarUInt64ValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarUInt64ValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarFloatValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarFloatValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarDoubleValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarDoubleValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarStringValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarStringValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarDateTimeValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarDateTimeValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarGuidValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarGuidValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarByteStringValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarByteStringValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarXmlElementValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarXmlElementValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarNodeIdValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarNodeIdValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarExpandedNodeIdValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarExpandedNodeIdValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarQualifiedNameValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarQualifiedNameValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarLocalizedTextValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarLocalizedTextValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarStatusCodeValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarStatusCodeValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarVariantValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarVariantValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarEnumerationValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarEnumerationValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarStructuredValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarStructuredValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarNumberValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarNumberValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarIntegerValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarIntegerValueVariableTestAsync();
        }

        [Fact]
        public Task NodeWriteStaticScalarUIntegerValueVariableTestAsync()
        {
            return GetTests().NodeWriteStaticScalarUIntegerValueVariableTestAsync();
        }
    }
}

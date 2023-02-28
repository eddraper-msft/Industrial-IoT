// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Azure.IIoT.OpcUa.Services.WebApi.Controllers.TestData.Binary
{
    using Azure.IIoT.OpcUa.Services.WebApi;
    using Azure.IIoT.OpcUa.Services.WebApi.Clients;
    using Azure.IIoT.OpcUa.Publisher.Stack;
    using Azure.IIoT.OpcUa.Testing.Fixtures;
    using Azure.IIoT.OpcUa.Testing.Tests;
    using Furly.Extensions.Serializers;
    using System.Threading.Tasks;
    using Xunit;

    [Collection(ReadCollection.Name)]
    public class ReadScalarTests : IClassFixture<WebAppFixture>
    {
        public ReadScalarTests(WebAppFixture factory, TestDataServer server)
        {
            _factory = factory;
            _server = server;
        }

        private ReadScalarValueTests<string> GetTests()
        {
            var client = _factory.CreateClient(); // Call to create server
            var registry = _factory.Resolve<IEndpointManager>();
            var endpointId = registry.RegisterEndpointAsync(_server.GetConnection().Endpoint).Result;
            var serializer = _factory.Resolve<IBinarySerializer>();
            return new ReadScalarValueTests<string>(() => // Create an adapter over the api
                new TwinWebApiAdapter(
                    new ControllerTestClient(_factory,
                    new TestConfig(client.BaseAddress), serializer)), endpointId,
                    (ep, n, s) => _server.Client.ReadValueAsync(_server.GetConnection(), n, s));
        }

        private readonly WebAppFixture _factory;
        private readonly TestDataServer _server;

        [Fact]
        public Task NodeReadStaticScalarBooleanValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarBooleanValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarSByteValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarSByteValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarByteValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarByteValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarInt16ValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarInt16ValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarUInt16ValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarUInt16ValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarInt32ValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarInt32ValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarUInt32ValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarUInt32ValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarInt64ValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarInt64ValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarUInt64ValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarUInt64ValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarFloatValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarFloatValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarDoubleValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarDoubleValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarStringValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarStringValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarDateTimeValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarDateTimeValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarGuidValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarGuidValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarByteStringValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarByteStringValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarXmlElementValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarXmlElementValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarNodeIdValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarNodeIdValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarExpandedNodeIdValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarExpandedNodeIdValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarQualifiedNameValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarQualifiedNameValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarLocalizedTextValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarLocalizedTextValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarStatusCodeValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarStatusCodeValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarVariantValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarVariantValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarEnumerationValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarEnumerationValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarStructuredValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarStructuredValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarNumberValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarNumberValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarIntegerValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarIntegerValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadStaticScalarUIntegerValueVariableTestAsync()
        {
            return GetTests().NodeReadStaticScalarUIntegerValueVariableTestAsync();
        }

        [Fact]
        public Task NodeReadDataAccessMeasurementFloatValueTestAsync()
        {
            return GetTests().NodeReadDataAccessMeasurementFloatValueTestAsync();
        }

        [Fact]
        public Task NodeReadDiagnosticsStatusTestAsync()
        {
            return GetTests().NodeReadDiagnosticsStatusTestAsync();
        }

        [Fact]
        public Task NodeReadDiagnosticsDebugTestAsync()
        {
            return GetTests().NodeReadDiagnosticsStatusTestAsync();
        }

        [Fact]
        public Task NodeReadDiagnosticsVerboseTestAsync()
        {
            return GetTests().NodeReadDiagnosticsStatusTestAsync();
        }
    }
}

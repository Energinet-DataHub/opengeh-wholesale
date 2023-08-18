// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Reflection;
using AutoFixture.Xunit2;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.WebApi.TestControllers;
using Energinet.DataHub.Wholesale.WebApi.V3;
using Energinet.DataHub.Wholesale.WebApi.V3.Batch;
using Energinet.DataHub.Wholesale.WebApi.V3.SettlementReport;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.WebApi;

public class ControllerRolesTests
{
    [Theory]
    [InlineAutoData(typeof(Test1Controller), "CreateTest", "TestRole")]
    [InlineAutoData(typeof(Test1Controller), "CreateTest2", "TestRole1")]
    [InlineAutoData(typeof(Test1Controller), "CreateTest2", "TestRole2")]
    [InlineAutoData(typeof(Test1Controller), "CreateTest3", "TestRole1, TestRole2")]
    [InlineAutoData(typeof(Test1Controller), "CreateTest4", null)]
    [InlineAutoData(typeof(Test2Controller), "", "TestRole")]
    [InlineAutoData(typeof(Test2Controller), "CreateTest5", "TestRole2")]
    public void TestEndpointsMustHaveCorrectPermissions(Type controllerType, string endpointRoute, string expectedPermissions)
    {
        // Arrange & Act
        var attributes = GetAuthorizeAttributesFromEndpoint(controllerType, endpointRoute);
        if (attributes == null)
        {
            Assert.True(false, $"The route {endpointRoute} does not exist in test controller {controllerType}.");
        }

        var actualPermissions = attributes.Select(x => x.Roles);

        // Assert
        Assert.NotNull(actualPermissions);
        actualPermissions.Should().Contain(expectedPermissions);
    }

    [Theory]
    [InlineAutoData(typeof(BatchController), "CreateBatch", Permissions.CalculationsManage)]
    [InlineAutoData(typeof(BatchController), "GetBatch", Permissions.CalculationsManage)]
    [InlineAutoData(typeof(BatchController), "SearchBatches", Permissions.CalculationsManage)]
    [InlineAutoData(typeof(SettlementReportController), "Download", Permissions.SettlementReportsManage)]
    [InlineAutoData(typeof(SettlementReportController), "GetSettlementReportAsStreamAsync", Permissions.SettlementReportsManage)]
    [InlineAutoData(typeof(SettlementReportController), "ZippedBasisDataStream", Permissions.SettlementReportsManage)]
    public void EndpointsMustHaveCorrectPermissions(Type controllerType, string endpointRoute, string expectedPermissions)
    {
        // Arrange & Act
        var attributes = GetAuthorizeAttributesFromEndpoint(controllerType, endpointRoute);
        if (attributes == null)
        {
            Assert.True(false, $"The route {endpointRoute} does not exist in controller {controllerType}.");
        }

        var actualPermissions = attributes.Select(x => x.Roles);

        // Assert
        actualPermissions.Should().Contain(expectedPermissions);
    }

    private static IEnumerable<AuthorizeAttribute> GetAuthorizeAttributesFromEndpoint(Type controllerType, string endpointRoute)
    {
        var authorizeAttribute = controllerType.GetCustomAttribute<AuthorizeAttribute>();
        var authorizeAttributes = new List<AuthorizeAttribute>();
        if (authorizeAttribute != null)
        {
            authorizeAttributes.Add(authorizeAttribute);
        }

        var authorizeAttributesFromMethods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.GetCustomAttribute<HttpMethodAttribute>()?.Name == endpointRoute ||
                        x.GetCustomAttribute<HttpMethodAttribute>()?.Template == endpointRoute)
            .SelectMany(x => x.GetCustomAttributes<AuthorizeAttribute>());

        authorizeAttributes.AddRange(authorizeAttributesFromMethods);
        return authorizeAttributes;
    }
}

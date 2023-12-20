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
using Energinet.DataHub.Wholesale.WebApi.V3;
using Energinet.DataHub.Wholesale.WebApi.V3.Batch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.WebApi;

public class ControllerAuthorizeAttributeTests
{
    [Fact]
    public void ControllerMethodsMustHaveAuthorizeAttributeIfControllerHasNot()
    {
        // Arrange & Act
        var controllers = GetControllersWithoutAuthorizeAttribute();
        var endpoints = GetEndpointsWithoutAuthorizeOrAllowAnonymousAttributes(controllers!);

        // Assert
        Assert.True(endpoints.Count == 0, $"Missing authorize attribute(s) found!\n {GenerateErrorMessage(endpoints)}");
    }

    private static IList<string> GetEndpointsWithoutAuthorizeOrAllowAnonymousAttributes(IEnumerable<Type> controllerTypes)
    {
        var methodsWithoutAuthorizeAttribute = new List<string>();
        foreach (var controller in controllerTypes)
        {
            var methods =
                controller.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.GetCustomAttributes<HttpMethodAttribute>().Any())
                    .Where(
                        action =>
                            action.GetCustomAttribute<AuthorizeAttribute>() == null &&
                            action.GetCustomAttribute<AllowAnonymousAttribute>() == null)
                    .ToList();

            if (!methods.IsNullOrEmpty())
            {
                methods.ForEach(x => methodsWithoutAuthorizeAttribute.Add($"{x.Name} in {controller.Name}."));
            }
        }

        return methodsWithoutAuthorizeAttribute;
    }

    private static string GenerateErrorMessage(ICollection<string> endpointsAuthorizationAttribute)
    {
        return endpointsAuthorizationAttribute.Count == 0 ? string.Empty : endpointsAuthorizationAttribute.Aggregate((a, b) => a + "\n" + b);
    }

    private static IEnumerable<Type>? GetControllersWithoutAuthorizeAttribute()
    {
        var controllers = Assembly.GetAssembly(typeof(CalculationController))?
            .GetTypes()
            .Where(type => typeof(V3ControllerBase).IsAssignableFrom(type) && !type.IsAbstract)
            .Where(t => t.GetCustomAttribute<AuthorizeAttribute>() == null)
            .ToList();
        return controllers;
    }
}

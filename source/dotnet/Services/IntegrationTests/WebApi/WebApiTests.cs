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

using Energinet.DataHub.Wholesale.IntegrationTests.Hosts;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BatchControllerV1 = Energinet.DataHub.Wholesale.WebApi.Controllers.V1.BatchController;
using BatchControllerV2 = Energinet.DataHub.Wholesale.WebApi.Controllers.V2.BatchController;

namespace Energinet.DataHub.Wholesale.IntegrationTests.WebApi;

[Collection(nameof(WebApiIntegrationTestHost))]
public class WebApiTests
{
    [Fact]
    public async Task ServiceCollection_CanResolveBatchControllerV1()
    {
        // Arrange
        using var host = await WebApiIntegrationTestHost.CreateAsync(collection => collection.AddScoped<BatchControllerV1>());

        await using var scope = host.BeginScope();

        // Act & Assert that the container can resolve the endpoints dependencies
        scope.ServiceProvider.GetRequiredService<BatchControllerV1>();
    }

    [Fact]
    public async Task ServiceCollection_CanResolveBatchControllerV2()
    {
        // Arrange
        using var host = await WebApiIntegrationTestHost.CreateAsync(collection => collection.AddScoped<BatchControllerV2>());

        await using var scope = host.BeginScope();

        // Act & Assert that the container can resolve the endpoints dependencies
        scope.ServiceProvider.GetRequiredService<BatchControllerV2>();
    }
}

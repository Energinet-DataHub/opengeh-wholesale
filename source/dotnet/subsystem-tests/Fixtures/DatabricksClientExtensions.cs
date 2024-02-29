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

using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Configuration;
using Microsoft.Azure.Databricks.Client;
using Microsoft.Azure.Databricks.Client.Models;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Fixtures;

/// <summary>
/// Convenient methods for using the <see cref="DatabricksClient"/>.
///
/// If we need to make several calls using the <see cref="DatabricksClient"/>,
/// we should not use these extensions but instead create and maintain an
/// instance of the client in the class where we use it.
///
/// Client documentation: https://github.com/Azure/azure-databricks-client
/// </summary>
public static class DatabricksClientExtensions
{
    /// <summary>
    /// Start Databricks SQL warehouse.
    /// It can be used to "warm up" the warehouse for tests to reduce timeouts.
    /// </summary>
    public static async Task StartWarehouseAsync(DatabricksWorkspaceConfiguration configuration)
    {
        using var client = DatabricksClient.CreateClient(configuration.BaseUrl, configuration.Token);
        await client.SQL.Warehouse.Start(configuration.WarehouseId);
    }

    /// <summary>
    /// Start Databricks SQL warehouse and wait for it to be in the specified state.
    /// </summary>
    public static async Task<bool> StartWarehouseAndWaitForWarehouseStateAsync(
        DatabricksWorkspaceConfiguration configuration,
        WarehouseState waitForState = WarehouseState.RUNNING,
        int waitTimeInMinutes = 10)
    {
        var delay = TimeSpan.FromSeconds(15);
        var waitTimeLimit = TimeSpan.FromMinutes(waitTimeInMinutes);
        using var databricksClient = DatabricksClient.CreateClient(configuration.BaseUrl, configuration.Token);
        await databricksClient.SQL.Warehouse.Start(configuration.WarehouseId);

        var isState = await Awaiter.TryWaitUntilConditionAsync(
            async () =>
            {
                var warehouseInfo = await databricksClient.SQL.Warehouse.Get(configuration.WarehouseId);
                return warehouseInfo.State == waitForState;
            },
            waitTimeLimit,
            delay);

        return isState;
    }

    /// <summary>
    /// Retrieve calculator job id from databricks.
    /// </summary>
    public static async Task<long> GetCalculatorJobIdAsync(this DatabricksClient databricksClient)
    {
        var jobs = await databricksClient.Jobs.List(name: "CalculatorJob");
        return jobs.Jobs
            .Single()
            .JobId;
    }
}

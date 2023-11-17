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

using Energinet.DataHub.Wholesale.DomainTests.Fixtures.Configuration;
using Microsoft.Azure.Databricks.Client;

namespace Energinet.DataHub.Wholesale.DomainTests.Fixtures
{
    /// <summary>
    /// Convenient methods for using the <see cref="DatabricksClient"/>.
    /// </summary>
    public static class DatabricksClientExtensions
    {
        /// <summary>
        /// Start Databricks SQL warehouse.
        /// It can be used to "warm up" the warehouse for tests to reduce timeouts.
        /// </summary>
        public static async Task StartkWarehouseAsync(DatabricksWorkspaceConfiguration configuration)
        {
            using var client = DatabricksClient.CreateClient(configuration.BaseUrl, configuration.Token);
            await client.SQL.Warehouse.Start(configuration.WarehouseId);
        }
    }
}

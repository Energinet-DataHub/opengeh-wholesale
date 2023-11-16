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

using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Wholesale.DomainTests.Fixtures.Configuration
{
    /// <summary>
    /// Configuration necessary to use the Databricks REST API and mange
    /// a SQL warehouse within a Databricks workspace.
    /// </summary>
    public class DatabricksWorkspaceConfiguration
    {
        public DatabricksWorkspaceConfiguration(string baseUrl, string token, string warehouseId)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException($"Cannot be null or whitespace.", nameof(baseUrl));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException($"Cannot be null or whitespace.", nameof(token));
            if (string.IsNullOrWhiteSpace(warehouseId))
                throw new ArgumentException($"Cannot be null or whitespace.", nameof(warehouseId));

            BaseUrl = baseUrl;
            Token = token;
            WarehouseId = warehouseId;
        }

        /// <summary>
        /// Workspace base url.
        /// </summary>
        public string BaseUrl { get; }

        /// <summary>
        /// Workspace token.
        /// </summary>
        public string Token { get; }

        public string WarehouseId { get; }

        internal static DatabricksWorkspaceConfiguration CreateFromConfiguration(IConfigurationRoot root)
        {
            return new DatabricksWorkspaceConfiguration(
                root.GetValue<string>("WorkspaceUrl")!,
                root.GetValue<string>("WorkspaceToken")!,
                root.GetValue<string>("WarehouseId")!);
        }
    }
}

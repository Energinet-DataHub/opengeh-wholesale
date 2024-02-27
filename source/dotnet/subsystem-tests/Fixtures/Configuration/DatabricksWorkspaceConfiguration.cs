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

namespace Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Configuration;

/// <summary>
/// Configuration necessary to use the Databricks REST API and mange
/// a SQL warehouse within a Databricks workspace.
/// </summary>
public sealed class DatabricksWorkspaceConfiguration
{
    private DatabricksWorkspaceConfiguration(string baseUrl, string token, string warehouseId)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("Cannot be null or whitespace.", nameof(baseUrl));
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Cannot be null or whitespace.", nameof(token));
        if (string.IsNullOrWhiteSpace(warehouseId))
            throw new ArgumentException("Cannot be null or whitespace.", nameof(warehouseId));

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

    /// <summary>
    /// SQL Warehouse id.
    /// </summary>
    public string WarehouseId { get; }

    /// <summary>
    /// Retrieve secrets from Key Vaults and create configuration.
    /// </summary>
    /// <param name="secretsConfiguration">A configuration that has been builded so it can retrieve secrets from both the shared and the internal key vault.</param>
    public static DatabricksWorkspaceConfiguration CreateFromConfiguration(IConfigurationRoot secretsConfiguration)
    {
        return new DatabricksWorkspaceConfiguration(
            $"https://{secretsConfiguration.GetValue<string>("dbw-workspace-url")!}",
            secretsConfiguration.GetValue<string>("dbw-workspace-token")!,
            secretsConfiguration.GetValue<string>("dbw-databricks-sql-endpoint-id")!);
    }
}

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

namespace Energinet.DataHub.Wholesale.Components.DatabricksClient;

public class DatabricksOptions
{
    /// <summary>
    /// Base URL for the databricks resource. For example: https://southcentralus.azuredatabricks.net.
    /// </summary>
    public string DATABRICKS_WORKSPACE_URL { get; set; } = string.Empty;

    /// <summary>
    /// The access token. To generate a token, refer to this document: https://docs.databricks.com/api/latest/authentication.html#generate-a-token.
    /// </summary>
    public string DATABRICKS_WORKSPACE_TOKEN { get; set; } = string.Empty;

    /// <summary>
    /// The databricks warehouse id.
    /// </summary>
    public string DATABRICKS_WAREHOUSE_ID { get; set; } = string.Empty;
}

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

using Microsoft.Azure.Databricks.Client;

namespace Energinet.DataHub.Wholesale.Infrastructure;

public interface IDatabricksWheelClient
{
    IJobsApi Jobs { get; }
}

public class DatabricksWheelClient : DatabricksClient, IDatabricksWheelClient
{
    protected DatabricksWheelClient(string baseUrl, string token, long timeoutSeconds = 30, Action<HttpClient> httpClientConfig = null!)
        : base(baseUrl, token, timeoutSeconds, httpClientConfig)
    {
    }

    protected DatabricksWheelClient(string baseUrl, string workspaceResourceId, string databricksToken, string managementToken, long timeoutSeconds = 30, Action<HttpClient> httpClientConfig = null!)
        : base(baseUrl, workspaceResourceId, databricksToken, managementToken, timeoutSeconds, httpClientConfig)
    {
    }

    public new IJobsApi Jobs => base.Jobs;
}

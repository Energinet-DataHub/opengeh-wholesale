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

using Newtonsoft.Json;

namespace Microsoft.Azure.Databricks.Client;

/// <summary>
/// Docker image connection information.
/// </summary>
public class DockerImage
{
    /// <summary>
    /// URL for the Docker image.
    /// </summary>
    [JsonProperty(PropertyName = "url")]
    public string Url { get; set; }

    /// <summary>
    /// Basic authentication information for Docker repository.
    /// </summary>
    [JsonProperty(PropertyName = "basic_auth")]
    public DockerBasicAuth BasicAuth { get; set; }
}

/// <summary>
/// Docker repository basic authentication information.
/// </summary>
#pragma warning disable SA1402
public class DockerBasicAuth
#pragma warning restore SA1402
{
    /// <summary>
    /// User name for the Docker repository.
    /// </summary>
    [JsonProperty(PropertyName = "username")]
    public string UserName { get; set; }

    /// <summary>
    /// Password for the Docker repository.
    /// </summary>
    [JsonProperty(PropertyName = "password")]
    public string Password { get; set; }
}

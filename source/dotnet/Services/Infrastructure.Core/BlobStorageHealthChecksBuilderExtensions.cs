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

using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Energinet.DataHub.Wholesale.Infrastructure.Core
{
    public static class BlobStorageHealthChecksBuilderExtensions
    {
        public static IHealthChecksBuilder AddBlobStorageContainerCheck(this IHealthChecksBuilder builder, string connectionString, string containerName)
        {
            return builder.AddAsyncCheck("BlobStorageContainer", async () =>
            {
                var client = new BlobServiceClient(connectionString);
                try
                {
                    var containersPageable = client.GetBlobContainersAsync();

                    await foreach (var blobContainerItem in containersPageable)
                    {
                        if (blobContainerItem.Name == containerName)
                            return HealthCheckResult.Healthy();
                    }

                    return HealthCheckResult.Unhealthy();
                }
                catch (Exception)
                {
                    return HealthCheckResult.Unhealthy();
                }
            });
        }
    }
}

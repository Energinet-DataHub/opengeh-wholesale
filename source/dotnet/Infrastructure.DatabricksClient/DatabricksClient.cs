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

using System.Net;
using System.Net.Http.Headers;

namespace Energinet.DataHub.Wholesale.Infrastructure.DatabricksClient
{
    /// <summary>
    /// A databricks client based on the DatabricksClient, which is using Job API 2.0.
    /// The client is extended with a method for reading jobs created using Python Wheels, using Job API 2.1.
    /// </summary>
    public sealed class DatabricksClient21 : IDisposable
    {
        private const string Version = "2.1";

        private DatabricksClient21(string baseUrl, string token, long timeoutSeconds = 30)
        {
            var apiUrl = new Uri(new Uri(baseUrl), $"api/{Version}/");

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            };

            var httpClient = new HttpClient(handler, false)
            {
                BaseAddress = apiUrl,
                Timeout = TimeSpan.FromSeconds(timeoutSeconds),
            };

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            Jobs = new JobsApiClient21(httpClient);
        }

        /// <summary>
        /// Create client object with specified base URL, access token and timeout.
        /// </summary>
        /// <param name="baseUrl">Base URL for the databricks resource. For example: https://southcentralus.azuredatabricks.net</param>
        /// <param name="token">The access token. To generate a token, refer to this document: https://docs.databricks.com/api/latest/authentication.html#generate-a-token </param>
        /// <param name="timeoutSeconds">Web request time out in seconds</param>
        public static DatabricksClient21 CreateClient(string baseUrl, string token, long timeoutSeconds = 30)
        {
            return new DatabricksClient21(baseUrl, token, timeoutSeconds);
        }

        public IJobsApi21 Jobs { get; }

        public void Dispose()
        {
            Jobs.Dispose();
        }
    }
}

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
using Newtonsoft.Json;

namespace Energinet.DataHub.Wholesale.Infrastructure.DatabricksClient.Api.V2
{
    public class WheelJob
    {
        /// <summary>
        /// The canonical identifier for this job.
        /// </summary>
        [JsonProperty(PropertyName = "job_id")]
        public long JobId { get; set; }

        /// <summary>
        /// The creator user name. This field won’t be included in the response if the user has already been deleted.
        /// </summary>
        [JsonProperty(PropertyName = "creator_user_name")]
        public string CreatorUserName { get; set; }

        /// <summary>
        /// Settings for this job and all of its runs. These settings can be updated using the resetJob method.
        /// </summary>
        [JsonProperty(PropertyName = "settings")]
        public WheelJobSettings Settings { get; set; }

        /// <summary>
        /// The time at which this job was created in epoch milliseconds (milliseconds since 1/1/1970 UTC).
        /// </summary>
        [JsonProperty(PropertyName = "created_time")]
        [JsonConverter(typeof(MillisecondEpochDateTimeConverter))]
        public DateTimeOffset CreatedTime { get; set; }
    }
}

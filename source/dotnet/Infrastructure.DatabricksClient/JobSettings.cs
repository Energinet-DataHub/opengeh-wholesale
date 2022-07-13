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

namespace Energinet.DataHub.Wholesale.Infrastructure.DatabricksClient
{
    /// <summary>
    /// Settings for a job. These settings can be updated using the resetJob method.
    /// </summary>
    public class JobSettings21 : RunSettings21
    {
        /// <summary>
        /// An optional name for the job. The default value is Untitled.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// An optional set of email addresses that will be notified when runs of this job begin or complete as well as when this job is deleted. The default behavior is to not send any emails.
        /// </summary>
        [JsonProperty(PropertyName = "email_notifications")]
        public JobEmailNotifications EmailNotifications { get; set; }

        /// <summary>
        /// An optional maximum number of times to retry an unsuccessful run. A run is considered to be unsuccessful if it completes with a FAILED result_state or INTERNAL_ERROR life_cycle_state. The value -1 means to retry indefinitely and the value 0 means to never retry. The default behavior is to never retry.
        /// </summary>
        [JsonProperty(PropertyName = "max_retries")]
        public int MaxRetries { get; set; }

        /// <summary>
        /// An optional minimal interval in milliseconds between attempts. The default behavior is that unsuccessful runs are immediately retried.
        /// </summary>
        [JsonProperty(PropertyName = "min_retry_interval_millis")]
        public int MinRetryIntervalMilliSeconds { get; set; }

        /// <summary>
        /// An optional policy to specify whether to retry a job when it times out. The default behavior is to not retry on timeout.
        /// </summary>
        [JsonProperty(PropertyName = "retry_on_timeout")]
        public bool RetryOnTimeout { get; set; }

        /// <summary>
        /// An optional maximum allowed number of concurrent runs of the job.
        /// Set this value if you want to be able to execute multiple runs of the same job concurrently. This is useful for example if you trigger your job on a frequent schedule and want to allow consecutive runs to overlap with each other, or if you want to trigger multiple runs which differ by their input parameters.
        /// This setting affects only new runs. For example, suppose the job’s concurrency is 4 and there are 4 concurrent active runs. Then setting the concurrency to 3 won’t kill any of the active runs. However, from then on, new runs will be skipped unless there are fewer than 3 active runs.
        /// This value cannot exceed 1000. Setting this value to 0 will cause all new runs to be skipped. The default behavior is to allow only 1 concurrent run.
        /// </summary>
        [JsonProperty(PropertyName = "max_concurrent_runs")]
        public int? MaxConcurrentRuns { get; set; }
    }
}

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

using Energinet.DataHub.Wholesale.Domain.BatchAggregate;

namespace Energinet.DataHub.Wholesale.Application.JobRunner;

public interface ICalculatorJobRunner
{
    Task<JobState> GetJobStateAsync(JobRunId jobRunId);

    /// <summary>
    /// Start job.
    /// </summary>
    /// <param name="jobParameters">
    /// Parameters must be on the form "--param-name=param-value".
    /// Further details about the format depends on the actual job runner implementation.
    /// </param>
    Task<JobRunId> SubmitJobAsync(IEnumerable<string> jobParameters);
}

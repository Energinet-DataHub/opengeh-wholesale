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

using Energinet.DataHub.Core.Databricks.Jobs.Abstractions;
using Microsoft.Azure.Databricks.Client.Models;

namespace Energinet.DataHub.Wholesale.Calculations.Infrastructure.Calculations;

public sealed class DatabricksCalculatorJobSelector : IDatabricksCalculatorJobSelector
{
    private readonly IJobsApiClient _client;

    public DatabricksCalculatorJobSelector(IJobsApiClient client)
    {
        _client = client;
    }

    public async Task<Job> GetAsync()
    {
        var jobs = await _client.Jobs.List().ConfigureAwait(false);
        var calculatorJob = jobs.Jobs.Single(j => j.Settings.Name == "CalculatorJob");
        return await _client.Jobs.Get(calculatorJob.JobId).ConfigureAwait(false);
    }
}

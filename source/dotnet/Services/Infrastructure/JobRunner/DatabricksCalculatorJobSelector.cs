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

using Energinet.DataHub.Wholesale.Components.DatabricksClient;

namespace Energinet.DataHub.Wholesale.Infrastructure.JobRunner;

public sealed class DatabricksCalculatorJobSelector : IDatabricksCalculatorJobSelector
{
    private readonly IDatabricksWheelClient _wheelClient;

    public DatabricksCalculatorJobSelector(IDatabricksWheelClient wheelClient)
    {
        _wheelClient = wheelClient;
    }

    public async Task<WheelJob> GetAsync()
    {
        var jobs = await _wheelClient.Jobs.List().ConfigureAwait(false);
        var calculatorJob = jobs.Single(j => j.Settings.Name == "CalculatorJob");
        return await _wheelClient.Jobs.GetWheel(calculatorJob.JobId).ConfigureAwait(false);
    }
}

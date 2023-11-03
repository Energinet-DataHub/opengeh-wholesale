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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Logging;
using Energinet.DataHub.Wholesale.Common.Models;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.CalculationResults.Application.RequestCalculationResult;

public class RequestCalculationResult : IRequestCalculationResult
{
    private readonly ILogger<RequestCalculationResult> _logger;
    private readonly IRequestCalculationResultQueries _requestCalculationResultQueries;

    public RequestCalculationResult(ILogger<RequestCalculationResult> logger, IRequestCalculationResultQueries requestCalculationResultQueries)
    {
        _logger = logger;
        _requestCalculationResultQueries = requestCalculationResultQueries;
    }

    public async Task<EnergyResult?> GetRequestCalculationResultAsync(IEnergyResultFilter filter, RequestedProcessType requestedProcessType)
    {
        var processType = await GetProcessTypeAsync(filter, requestedProcessType).ConfigureAwait(false);

        var query = new EnergyResultQuery(filter, processType);

        var calculationResult = await _requestCalculationResultQueries.GetAsync(query).ConfigureAwait(false);

        _logger.LogDebug("Found {CalculationResult} calculation results based on {Query} query.", calculationResult?.ToJsonString(), query.ToJsonString());
        return calculationResult;
    }

    private Task<ProcessType> GetProcessTypeAsync(IEnergyResultFilter filter, RequestedProcessType requestedProcessType)
    {
        if (requestedProcessType == RequestedProcessType.LatestCorrection)
            return _requestCalculationResultQueries.GetLatestCorrectionAsync(filter);

        var processType = ProcessTypeMapper.FromRequestedProcessType(requestedProcessType);
        return Task.FromResult(processType);
    }
}

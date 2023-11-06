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

public class RequestCalculationResultRetriever : IRequestCalculationResultRetriever
{
    private readonly ILogger<RequestCalculationResultRetriever> _logger;
    private readonly IRequestCalculationResultQueries _requestCalculationResultQueries;

    public RequestCalculationResultRetriever(ILogger<RequestCalculationResultRetriever> logger, IRequestCalculationResultQueries requestCalculationResultQueries)
    {
        _logger = logger;
        _requestCalculationResultQueries = requestCalculationResultQueries;
    }

    public async IAsyncEnumerable<EnergyResult> GetRequestCalculationResultAsync(IEnergyResultFilter filter, RequestedProcessType requestedProcessType)
    {
        var processType = await GetSpecificProcessTypeAsync(filter, requestedProcessType).ConfigureAwait(false);

        var query = new EnergyResultQuery(filter, processType);

        var calculationResultCount = 0;
        await foreach (var calculationResult in _requestCalculationResultQueries.GetAsync(query))
        {
            yield return calculationResult;
            calculationResultCount++;
        }

        _logger.LogDebug("Found {CalculationResults} calculation results based on {Query} query.", calculationResultCount, query.ToJsonString());
    }

    private Task<ProcessType> GetSpecificProcessTypeAsync(IEnergyResultFilter filter, RequestedProcessType requestedProcessType)
    {
        if (requestedProcessType == RequestedProcessType.LatestCorrection)
            return _requestCalculationResultQueries.GetLatestCorrectionAsync(filter);

        var processType = ProcessTypeMapper.FromRequestedProcessType(requestedProcessType);
        return Task.FromResult(processType);
    }
}

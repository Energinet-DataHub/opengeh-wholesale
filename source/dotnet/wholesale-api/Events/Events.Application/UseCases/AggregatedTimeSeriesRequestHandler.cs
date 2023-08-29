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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.Application.UseCases;

public class AggregatedTimeSeriesRequestHandler : IAggregatedTimeSeriesRequestHandler
{
    private readonly ICalculationResultQueries _calculationResultQueries;
    private readonly ServiceBusClient _servicebusclient;

    public AggregatedTimeSeriesRequestHandler(ICalculationResultQueries calculationResultQueries)
    {
        _calculationResultQueries = calculationResultQueries;
        _servicebusclient = new ServiceBusClient("connectionstring");
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var processor = _servicebusclient.CreateProcessor("queueName");
        try
        {
            processor.ProcessMessageAsync += ProcessMessageAsync;
            processor.ProcessErrorAsync += ProcessErrorAsync;
        }
        finally
        {
            await processor.DisposeAsync().ConfigureAwait(false);
            await _servicebusclient.DisposeAsync().ConfigureAwait(false);
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
    {
        throw new NotImplementedException();
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs arg)
    {
        // create the request from the protobuf message
        // call the query service
        var timeseriesRequested =
            _calculationResultQueries.GetAsync(Instant.MinValue, Instant.MaxValue, TimeSeriesType.TotalProduction);
        // create the response
        // send the response to EDI inbox.
        await arg.CompleteMessageAsync(arg.Message).ConfigureAwait(false);
        throw new NotImplementedException();
    }
}

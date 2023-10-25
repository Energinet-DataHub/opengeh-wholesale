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

using Energinet.DataHub.Core.Messaging.Communication.Internal;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Models;
using Energinet.DataHub.Wholesale.Events.Application.Communication;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EventProviders
{
    public class WholesaleResultEventProvider : ResultEventProvider, IWholesaleResultEventProvider
    {
        private readonly IWholesaleResultQueries _wholesaleResultQueries;
        private readonly IAmountPerChargeResultProducedV1Factory _amountPerChargeResultProducedV1Factory;
        private readonly IMonthlyAmountPerChargeResultProducedV1Factory _monthlyAmountPerChargeResultProducedV1Factory;

        public WholesaleResultEventProvider(
            IWholesaleResultQueries wholesaleResultQueries,
            IAmountPerChargeResultProducedV1Factory amountPerChargeResultProducedV1Factory,
            IMonthlyAmountPerChargeResultProducedV1Factory monthlyAmountPerChargeResultProducedV1Factory)
        {
            _wholesaleResultQueries = wholesaleResultQueries;
            _amountPerChargeResultProducedV1Factory = amountPerChargeResultProducedV1Factory;
            _monthlyAmountPerChargeResultProducedV1Factory = monthlyAmountPerChargeResultProducedV1Factory;
        }

        public bool CanContainWholesaleResults(CompletedBatch batch)
        {
            return batch.ProcessType
                is ProcessType.WholesaleFixing
                or ProcessType.FirstCorrectionSettlement
                or ProcessType.SecondCorrectionSettlement
                or ProcessType.ThirdCorrectionSettlement;
        }

        public async IAsyncEnumerable<IntegrationEvent> GetAsync(CompletedBatch batch, EventProviderState state)
        {
            await foreach (var wholesaleResult in _wholesaleResultQueries.GetAsync(batch.Id).ConfigureAwait(false))
            {
                state.EventCount++;
                yield return CreateEventFromWholesaleResult(wholesaleResult);
            }
        }

        private IntegrationEvent CreateEventFromWholesaleResult(WholesaleResult wholesaleResult)
        {
            return wholesaleResult.ChargeResolution switch
            {
                ChargeResolution.Day or ChargeResolution.Hour =>
                    CreateIntegrationEvent(_amountPerChargeResultProducedV1Factory.Create(wholesaleResult)),
                ChargeResolution.Month =>
                    CreateIntegrationEvent(_monthlyAmountPerChargeResultProducedV1Factory.Create(wholesaleResult)),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(wholesaleResult.ChargeResolution),
                    actualValue: wholesaleResult.ChargeResolution,
                    "Unexpected resolution."),
            };
        }
    }
}

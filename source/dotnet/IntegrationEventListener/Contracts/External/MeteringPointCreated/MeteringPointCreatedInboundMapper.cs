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

using System.ComponentModel;
using Energinet.DataHub.Core.Messaging.Protobuf;
using Energinet.DataHub.Core.Messaging.Transport;
using Energinet.DataHub.Wholesale.Application.MeteringPoints;
using NodaTime;
using mpTypes = Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types;

namespace Energinet.DataHub.Wholesale.IntegrationEventListener.Contracts.External.MeteringPointCreated
{
    public class MeteringPointCreatedInboundMapper : ProtobufInboundMapper<
        Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated>
    {
        public static SettlementMethod? MapSettlementMethod(mpTypes.SettlementMethod settlementMethod) =>
            settlementMethod switch
            {
                mpTypes.SettlementMethod.SmFlex => SettlementMethod.Flex,
                mpTypes.SettlementMethod.SmProfiled => SettlementMethod.Profiled,
                mpTypes.SettlementMethod.SmNonprofiled => SettlementMethod.NonProfiled,
                mpTypes.SettlementMethod.SmNull => null,
                _ => throw new InvalidEnumArgumentException(
                    $"Provided SettlementMethod value '{settlementMethod}' is invalid and cannot be mapped."),
            };

        public static ConnectionState MapConnectionState(mpTypes.ConnectionState connectionState) => connectionState switch
        {
            mpTypes.ConnectionState.CsNew => ConnectionState.New,
            _ => throw new InvalidEnumArgumentException(
                $"Provided ConnectionState value '{connectionState}' is invalid and cannot be mapped."),
        };

        public static MeteringPointType MapMeteringPointType(mpTypes.MeteringPointType meteringPointType) => meteringPointType switch
        {
            mpTypes.MeteringPointType.MptAnalysis => MeteringPointType.Analysis,
            mpTypes.MeteringPointType.MptConsumption => MeteringPointType.Consumption,
            mpTypes.MeteringPointType.MptExchange => MeteringPointType.Exchange,
            mpTypes.MeteringPointType.MptProduction => MeteringPointType.Production,
            mpTypes.MeteringPointType.MptVeproduction => MeteringPointType.VeProduction,
            mpTypes.MeteringPointType.MptElectricalHeating => MeteringPointType.ElectricalHeating,
            mpTypes.MeteringPointType.MptInternalUse => MeteringPointType.InternalUse,
            mpTypes.MeteringPointType.MptNetConsumption => MeteringPointType.NetConsumption,
            mpTypes.MeteringPointType.MptNetProduction => MeteringPointType.NetProduction,
            mpTypes.MeteringPointType.MptOtherConsumption => MeteringPointType.OtherConsumption,
            mpTypes.MeteringPointType.MptOtherProduction => MeteringPointType.OtherProduction,
            mpTypes.MeteringPointType.MptOwnProduction => MeteringPointType.OwnProduction,
            mpTypes.MeteringPointType.MptTotalConsumption => MeteringPointType.TotalConsumption,
            mpTypes.MeteringPointType.MptWholesaleServices => MeteringPointType.WholesaleService,
            mpTypes.MeteringPointType.MptConsumptionFromGrid => MeteringPointType.ConsumptionFromGrid,
            mpTypes.MeteringPointType.MptExchangeReactiveEnergy => MeteringPointType.ExchangeReactiveEnergy,
            mpTypes.MeteringPointType.MptGridLossCorrection => MeteringPointType.GridLossCorrection,
            mpTypes.MeteringPointType.MptNetFromGrid => MeteringPointType.NetFromGrid,
            mpTypes.MeteringPointType.MptNetToGrid => MeteringPointType.NetToGrid,
            mpTypes.MeteringPointType.MptSupplyToGrid => MeteringPointType.SupplyToGrid,
            mpTypes.MeteringPointType.MptSurplusProductionGroup => MeteringPointType.SurplusProductionGroup,
            _ => throw new InvalidEnumArgumentException(
                    $"Provided MeteringPointType value '{meteringPointType}' is invalid and cannot be mapped."),
        };

        protected override IInboundMessage Convert(Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated meteringPointCreated)
        {
            var settlementMethod = MapSettlementMethod(meteringPointCreated.SettlementMethod);
            var connectionState = MapConnectionState(meteringPointCreated.ConnectionState);
            var meteringPointType = MapMeteringPointType(meteringPointCreated.MeteringPointType);

            var instant = Instant.FromUnixTimeSeconds(meteringPointCreated.EffectiveDate.Seconds);
            var plusNanoseconds = instant.PlusNanoseconds(meteringPointCreated.EffectiveDate.Nanos);

            return new MeteringPointCreatedEvent(
                meteringPointCreated.GsrnNumber,
                Guid.Parse(meteringPointCreated.GridAreaCode), // The GridAreaLinkId name is wrong - it's a grid area link id
                settlementMethod,
                connectionState,
                plusNanoseconds,
                meteringPointType);
        }
    }
}

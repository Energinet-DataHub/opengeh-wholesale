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
using Energinet.DataHub.Core.App.Common.Abstractions.IntegrationEventContext;
using Energinet.DataHub.Wholesale.IntegrationEventListener.Extensions;
using mpTypes = Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types;

namespace Energinet.DataHub.Wholesale.IntegrationEventListener.MeteringPoints;

public class MeteringPointCreatedDtoFactory
{
    private readonly IIntegrationEventContext _integrationEventContext;

    public MeteringPointCreatedDtoFactory(IIntegrationEventContext integrationEventContext)
    {
        _integrationEventContext = integrationEventContext;
    }

    public MeteringPointCreatedDto Create(DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated meteringPointCreated)
    {
        if (meteringPointCreated == null)
            throw new InvalidOperationException(nameof(meteringPointCreated));

        var settlementMethod = MapSettlementMethod(meteringPointCreated.SettlementMethod);
        var connectionState = MapConnectionState(meteringPointCreated.ConnectionState);
        var meteringPointType = MapMeteringPointType(meteringPointCreated.MeteringPointType);
        var resolution = MapResolutionType(meteringPointCreated.MeterReadingPeriodicity);

        if (_integrationEventContext.TryReadMetadata(out var eventMetadata))
        {
            return new MeteringPointCreatedDto(
                meteringPointCreated.GsrnNumber,
                Guid.Parse(meteringPointCreated.GridAreaCode), // The GridAreaLinkId name is wrong - it's a grid area link id
                settlementMethod,
                connectionState,
                meteringPointCreated.EffectiveDate.ToInstant(),
                meteringPointType,
                eventMetadata.MessageType,
                eventMetadata.OperationTimestamp,
                resolution);
        }

        throw new InvalidOperationException($"Could not read metadata for integration event in {nameof(MeteringPointCreatedDtoFactory)}.");
    }

    public static SettlementMethod? MapSettlementMethod(mpTypes.SettlementMethod settlementMethod)
    {
        return settlementMethod switch
        {
            mpTypes.SettlementMethod.SmFlex => SettlementMethod.Flex,
            mpTypes.SettlementMethod.SmProfiled => SettlementMethod.Profiled,
            mpTypes.SettlementMethod.SmNonprofiled => SettlementMethod.NonProfiled,
            mpTypes.SettlementMethod.SmNull => null,
            _ => throw new InvalidEnumArgumentException($"Provided SettlementMethod value '{settlementMethod}' is invalid and cannot be mapped."),
        };
    }

    public static ConnectionState MapConnectionState(mpTypes.ConnectionState connectionState)
    {
        return connectionState switch
        {
            mpTypes.ConnectionState.CsNew => ConnectionState.New,
            _ => throw new InvalidEnumArgumentException($"Provided ConnectionState value '{connectionState}' is invalid and cannot be mapped."),
        };
    }

    public static Resolution MapResolutionType(mpTypes.MeterReadingPeriodicity readingPeriodicity)
    {
        return readingPeriodicity switch
        {
            mpTypes.MeterReadingPeriodicity.MrpHourly => Resolution.Hourly,
            mpTypes.MeterReadingPeriodicity.MrpQuarterly => Resolution.Quaterly,
            _ => throw new InvalidEnumArgumentException($"Provided ConnectionState value '{readingPeriodicity}' is invalid and cannot be mapped."),
        };
    }

    public static MeteringPointType MapMeteringPointType(mpTypes.MeteringPointType meteringPointType)
    {
        return meteringPointType switch
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
            _ => throw new InvalidEnumArgumentException($"Provided MeteringPointType value '{meteringPointType}' is invalid and cannot be mapped."),
        };
    }
}

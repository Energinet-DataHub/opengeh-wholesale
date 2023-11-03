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
using Energinet.DataHub.Wholesale.EDI.Mappers;
using Energinet.DataHub.Wholesale.EDI.Models;
using NodaTime;
using NodaTime.Text;
using Period = Energinet.DataHub.Wholesale.EDI.Models.Period;

namespace Energinet.DataHub.Wholesale.EDI.Factories;

public class AggregatedTimeSeriesRequestFactory : IAggregatedTimeSeriesRequestFactory
{
    private readonly RequestedProcessTypeMapper _requestedProcessTypeMapper;

    public AggregatedTimeSeriesRequestFactory(RequestedProcessTypeMapper requestedProcessTypeMapper)
    {
        _requestedProcessTypeMapper = requestedProcessTypeMapper;
    }

    public AggregatedTimeSeriesRequest Parse(Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest request)
    {
        return new AggregatedTimeSeriesRequest(
            MapPeriod(request.Period),
            TimeSeriesTypeMapper.MapTimeSeriesType(request.MeteringPointType, request.SettlementMethod),
            MapAggregationPerRoleAndGridArea(request),
            _requestedProcessTypeMapper.ToRequestedProcessType(request.BusinessReason, request.HasSettlementSeriesVersion ? request.SettlementSeriesVersion : null));
    }

    private AggregationPerRoleAndGridArea MapAggregationPerRoleAndGridArea(Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest request)
    {
        return request.AggregationLevelCase switch
        {
            Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest.AggregationLevelOneofCase.AggregationPerGridarea =>
                new AggregationPerRoleAndGridArea(GridAreaCode: request.AggregationPerGridarea.GridAreaCode),
            Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest.AggregationLevelOneofCase.AggregationPerEnergysupplierPerGridarea =>
                new AggregationPerRoleAndGridArea(
                    GridAreaCode: request.AggregationPerEnergysupplierPerGridarea.GridAreaCode,
                    EnergySupplierId: request.AggregationPerEnergysupplierPerGridarea.EnergySupplierId),
            Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest.AggregationLevelOneofCase.AggregationPerBalanceresponsiblepartyPerGridarea =>
                new AggregationPerRoleAndGridArea(
                    GridAreaCode: request.AggregationPerBalanceresponsiblepartyPerGridarea.GridAreaCode,
                    BalanceResponsibleId: request.AggregationPerBalanceresponsiblepartyPerGridarea.BalanceResponsiblePartyId),
            Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest.AggregationLevelOneofCase.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea =>
                new AggregationPerRoleAndGridArea(
                    GridAreaCode: request.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.GridAreaCode,
                    EnergySupplierId: request.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.EnergySupplierId,
                    BalanceResponsibleId: request.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.BalanceResponsiblePartyId),
            _ => throw new InvalidOperationException("Unknown aggregation level"),
        };
    }

    private Period MapPeriod(Energinet.DataHub.Edi.Requests.Period period)
    {
        return new Period(
        InstantPattern.General.Parse(period.Start).Value,
        InstantPattern.General.Parse(period.End).Value);
    }
}

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

using Energinet.DataHub.Edi.Requests;
using Energinet.DataHub.Wholesale.EDI.Models;
using Energinet.DataHub.Wholesale.EDI.UnitTests.Validators;
using NodaTime;
using AggregatedTimeSeriesRequest = Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Builders;

public class AggregatedTimeSeriesRequestBuilder
{
    private string _meteringPointType = MeteringPointType.Production;

    private string _start;
    private string _end;
    private string? _energySupplierId;
    private string _requestedByActorRoleId;
    private string _requestedByActorId;
    private string? _settlementMethod;
    private string? _balanceResponsibleId;
    private string? _settlementSeriesVersion;
    private string _businessReason;
    private string? _gridAreaCode;

    private AggregatedTimeSeriesRequestBuilder()
    {
        var now = SystemClock.Instance.GetCurrentInstant();
        _start = Instant.FromUtc(now.InUtc().Year, 1, 1, 23, 0, 0).ToString();
        _end = Instant.FromUtc(now.InUtc().Year, 1, 2, 23, 0, 0).ToString();
        _requestedByActorRoleId = ActorRoleCode.EnergySupplier;
        _requestedByActorId = EnergySupplierValidatorTest.ValidGlnNumber;
        _energySupplierId = _requestedByActorId;
        _businessReason = BusinessReason.WholesaleFixing;
    }

    public static AggregatedTimeSeriesRequestBuilder AggregatedTimeSeriesRequest()
    {
        return new AggregatedTimeSeriesRequestBuilder();
    }

    public AggregatedTimeSeriesRequest Build()
    {
        var request = new AggregatedTimeSeriesRequest
        {
            Period = new DataHub.Edi.Requests.Period()
            {
                Start = _start,
                End = _end,
            },
            MeteringPointType = _meteringPointType,
            RequestedByActorRole = _requestedByActorRoleId,
            RequestedByActorId = _requestedByActorId,
            BusinessReason = _businessReason,
        };

        if (_energySupplierId != null)
            request.EnergySupplierId = _energySupplierId;

        if (_balanceResponsibleId != null)
            request.BalanceResponsibleId = _balanceResponsibleId;

        if (_gridAreaCode != null)
            request.GridAreaCode = _gridAreaCode;

        if (_settlementMethod != null)
            request.SettlementMethod = _settlementMethod;

        if (_settlementSeriesVersion != null)
            request.SettlementSeriesVersion = _settlementSeriesVersion;

        return request;
    }

    public AggregatedTimeSeriesRequestBuilder WithStartDate(string start)
    {
        _start = start;
        return this;
    }

    public AggregatedTimeSeriesRequestBuilder WithEndDate(string end)
    {
        _end = end;
        return this;
    }

    public AggregatedTimeSeriesRequestBuilder WithEnergySupplierId(string? energySupplierId)
    {
        _energySupplierId = energySupplierId;
        return this;
    }

    public AggregatedTimeSeriesRequestBuilder WithRequestedByActorId(string actorId)
    {
        _requestedByActorId = actorId;
        return this;
    }

    public AggregatedTimeSeriesRequestBuilder WithRequestedByActorRole(string actorRoleId)
    {
        _requestedByActorRoleId = actorRoleId;
        return this;
    }

    public AggregatedTimeSeriesRequestBuilder WithMeteringPointType(string meteringPointType)
    {
        _meteringPointType = meteringPointType;
        return this;
    }

    public AggregatedTimeSeriesRequestBuilder WithSettlementMethod(string? settlementMethod)
    {
        _settlementMethod = settlementMethod;

        return this;
    }

    public AggregatedTimeSeriesRequestBuilder WithBalanceResponsibleId(string? balanceResponsibleId)
    {
        _balanceResponsibleId = balanceResponsibleId;
        return this;
    }

    public AggregatedTimeSeriesRequestBuilder WithSettlementSeriesVersion(string? settlementSeriesVersion)
    {
        _settlementSeriesVersion = settlementSeriesVersion;
        return this;
    }

    public AggregatedTimeSeriesRequestBuilder WithBusinessReason(string businessReason)
    {
        _businessReason = businessReason;
        return this;
    }

    public AggregatedTimeSeriesRequestBuilder WithGridArea(string gridArea)
    {
        _gridAreaCode = gridArea;
        return this;
    }
}

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

using Energinet.DataHub.Wholesale.Edi.Contracts;
using Energinet.DataHub.Wholesale.Edi.UnitTests.Validators.AggregatedTimeSeriesRequest;
using NodaTime;
using AggregatedTimeSeriesRequest = Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Builders;

public class AggregatedTimeSeriesRequestBuilder
{
    private readonly List<string> _gridAreaCodes = [];
    private string? _meteringPointType = DataHubNames.MeteringPointType.Production;
    private string _start;
    private string _end;
    private string? _energySupplierId;
    private string _requestedByActorRole;
    private string _requestedByActorNumber;
    private string? _settlementMethod;
    private string? _balanceResponsibleId;
    private string? _settlementVersion;
    private string _businessReason;

    private AggregatedTimeSeriesRequestBuilder()
    {
        var now = SystemClock.Instance.GetCurrentInstant();
        _start = Instant.FromUtc(now.InUtc().Year, 1, 1, 23, 0, 0).ToString();
        _end = Instant.FromUtc(now.InUtc().Year, 1, 2, 23, 0, 0).ToString();
        _requestedByActorRole = DataHubNames.ActorRole.EnergySupplier;
        _requestedByActorNumber = EnergySupplierValidatorTest.ValidGlnNumber;
        _energySupplierId = _requestedByActorNumber;
        _businessReason = DataHubNames.BusinessReason.BalanceFixing;
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
            RequestedForActorRole = _requestedByActorRole,
            RequestedForActorNumber = _requestedByActorNumber,
            BusinessReason = _businessReason,
        };

        if (_meteringPointType != null)
            request.MeteringPointType = _meteringPointType;

        if (_energySupplierId != null)
            request.EnergySupplierId = _energySupplierId;

        if (_balanceResponsibleId != null)
            request.BalanceResponsibleId = _balanceResponsibleId;

        if (_gridAreaCodes.Count > 0)
            request.GridAreaCodes.AddRange(_gridAreaCodes);

        if (_settlementMethod != null)
            request.SettlementMethod = _settlementMethod;

        if (_settlementVersion != null)
            request.SettlementVersion = _settlementVersion;

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
        _requestedByActorNumber = actorId;
        return this;
    }

    public AggregatedTimeSeriesRequestBuilder WithRequestedByActorRole(string actorRole)
    {
        _requestedByActorRole = actorRole;
        return this;
    }

    public AggregatedTimeSeriesRequestBuilder WithMeteringPointType(string? meteringPointType)
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

    public AggregatedTimeSeriesRequestBuilder WithSettlementVersion(string? settlementVersion)
    {
        _settlementVersion = settlementVersion;
        return this;
    }

    public AggregatedTimeSeriesRequestBuilder WithBusinessReason(string businessReason)
    {
        _businessReason = businessReason;
        return this;
    }

    public AggregatedTimeSeriesRequestBuilder WithGridArea(string? gridArea)
    {
        if (gridArea == null)
            _gridAreaCodes.Clear();
        else
            _gridAreaCodes.Add(gridArea);

        return this;
    }
}

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
using ChargeType = Energinet.DataHub.Edi.Requests.ChargeType;
using WholesaleServicesRequest = Energinet.DataHub.Edi.Requests.WholesaleServicesRequest;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Builders;

public class WholesaleServicesRequestBuilder
{
    private string _requestedByActorId = EnergySupplierValidatorTest.ValidGlnNumber;
    private string _requestedByActorRole = DataHubNames.ActorRole.SystemOperator;
    private string _businessReason = DataHubNames.BusinessReason.WholesaleFixing;
    private string? _resolution;
    private string _periodStart = Instant.FromUtc(2023, 1, 31, 23, 0, 0).ToString();
    private string? _periodEnd = Instant.FromUtc(2023, 2, 28, 23, 0, 0).ToString();
    private string? _energySupplierId;
    private string? _chargeOwnerId;
    private string? _gridAreaCode;
    private string? _settlementVersion;
    private ChargeType[] _chargeType = Array.Empty<ChargeType>();

    public WholesaleServicesRequest Build()
    {
        var request = new WholesaleServicesRequest
        {
            RequestedForActorNumber = _requestedByActorId,
            RequestedForActorRole = _requestedByActorRole,
            BusinessReason = _businessReason,
            PeriodStart = _periodStart,
        };

        if (_resolution != null)
            request.Resolution = _resolution;
        if (_periodEnd != null)
            request.PeriodEnd = _periodEnd;
        if (_energySupplierId != null)
            request.EnergySupplierId = _energySupplierId;
        if (_chargeOwnerId != null)
            request.ChargeOwnerId = _chargeOwnerId;
        if (_gridAreaCode != null)
            request.GridAreaCodes.Add(_gridAreaCode);
        if (_settlementVersion != null)
            request.SettlementVersion = _settlementVersion;

        request.ChargeTypes.AddRange(_chargeType);

        return request;
    }

    public WholesaleServicesRequestBuilder WithRequestedByActorId(string requestedByActorId)
    {
        _requestedByActorId = requestedByActorId;
        return this;
    }

    public WholesaleServicesRequestBuilder WithRequestedByActorRole(string requestedByActorRole)
    {
        _requestedByActorRole = requestedByActorRole;
        return this;
    }

    public WholesaleServicesRequestBuilder WithBusinessReason(string businessReason)
    {
        _businessReason = businessReason;
        return this;
    }

    public WholesaleServicesRequestBuilder WithResolution(string? resolution)
    {
        _resolution = resolution;
        return this;
    }

    public WholesaleServicesRequestBuilder WithPeriodStart(string periodStart)
    {
        _periodStart = periodStart;
        return this;
    }

    public WholesaleServicesRequestBuilder WithPeriodEnd(string? periodEnd)
    {
        _periodEnd = periodEnd;
        return this;
    }

    public WholesaleServicesRequestBuilder WithEnergySupplierId(string energySupplierId)
    {
        _energySupplierId = energySupplierId;
        return this;
    }

    public WholesaleServicesRequestBuilder WithChargeOwnerId(string chargeOwnerId)
    {
        _chargeOwnerId = chargeOwnerId;
        return this;
    }

    public WholesaleServicesRequestBuilder WithSettlementVersion(string? settlementVersion)
    {
        _settlementVersion = settlementVersion;
        return this;
    }

    public WholesaleServicesRequestBuilder WithChargeTypes(params ChargeType[] chargeTypes)
    {
        _chargeType = chargeTypes;
        return this;
    }

    public WholesaleServicesRequestBuilder WithGridAreaCode(string gridAreaCode)
    {
        _gridAreaCode = gridAreaCode;
        return this;
    }
}

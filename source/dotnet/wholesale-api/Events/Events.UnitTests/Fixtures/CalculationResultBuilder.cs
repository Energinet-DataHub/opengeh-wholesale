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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Fixtures;

public sealed class CalculationResultBuilder
{
    private readonly long _version = DateTime.Now.Ticks;
    private TimeSeriesType _timeSeriesType = TimeSeriesType.Production;
    private EnergyTimeSeriesPoint[] _points = { new(DateTime.Now, 0, new List<QuantityQuality> { QuantityQuality.Measured }) };
    private Guid _batchId = Guid.NewGuid();
    private string? _energySupplierId;
    private string? _balanceResponsiblePartyId;

    public CalculationResultBuilder WithId(Guid batchId)
    {
        _batchId = batchId;
        return this;
    }

    public CalculationResultBuilder WithTimeSeriesType(TimeSeriesType timeSeriesType)
    {
        _timeSeriesType = timeSeriesType;
        return this;
    }

    public CalculationResultBuilder WithTimeSeriesPoints(EnergyTimeSeriesPoint[] timeSeriesPoints)
    {
        _points = timeSeriesPoints;
        return this;
    }

    public CalculationResultBuilder WithEnergySupplier()
    {
        _energySupplierId = "es";
        return this;
    }

    public CalculationResultBuilder WithBalanceResponsibleParty()
    {
        _balanceResponsiblePartyId = "brp";
        return this;
    }

    public EnergyResult Build()
    {
        return new EnergyResult(
            Guid.NewGuid(),
            _batchId,
            "543",
            _timeSeriesType,
            _energySupplierId,
            _balanceResponsiblePartyId,
            _points,
            ProcessType.Aggregation,
            Instant.FromUtc(2022, 12, 31, 23, 0),
            Instant.FromUtc(2023, 1, 31, 23, 0),
            null,
            _version);
    }
}

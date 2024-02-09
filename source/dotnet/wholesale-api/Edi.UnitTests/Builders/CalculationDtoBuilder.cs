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

using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Builders;

public class CalculationDtoBuilder
{
    private DateTimeOffset _periodStart;
    private DateTimeOffset _periodEnd;
    private DateTimeOffset _executionStart;
    private DateTimeOffset __executionEnd;
    private Guid _calculationId;
    private CalculationType _calculationType;
    private long _version;

    private CalculationDtoBuilder()
    {
        _periodStart = DateTimeOffset.Parse("2022-05-01T22:00Z");
        _periodEnd = DateTimeOffset.Parse("2022-05-31T22:00Z");
        _executionStart = DateTimeOffset.Parse("2022-06-01T22:00Z");
        __executionEnd = DateTimeOffset.Parse("2022-06-01T22:00Z");
        _calculationId = Guid.NewGuid();
        _calculationType = CalculationType.BalanceFixing;
        _version = 1;
    }

    public CalculationDto Build()
    {
        return new CalculationDto(
            1,
            _calculationId,
            _periodStart,
            _periodEnd,
            "PT15M",
            QuantityUnit.Kwh.ToString(),
            _executionStart,
            __executionEnd,
            CalculationState.Completed,
            false,
            new[] { "543" },
            _calculationType,
            Guid.NewGuid(),
            _version);
    }

    public static CalculationDtoBuilder CalculationDto()
    {
        return new CalculationDtoBuilder();
    }

    public CalculationDtoBuilder WithPeriodStart(Instant periodStart)
    {
        _periodStart = periodStart.ToDateTimeOffset();
        return this;
    }

    public CalculationDtoBuilder WithPeriodEnd(Instant periodEnd)
    {
        _periodEnd = periodEnd.ToDateTimeOffset();
        return this;
    }

    public CalculationDtoBuilder WithVersion(long version)
    {
        _version = version;
        return this;
    }

    public CalculationDtoBuilder WithCalculationType(CalculationType calculationType)
    {
        _calculationType = calculationType;
        return this;
    }
}

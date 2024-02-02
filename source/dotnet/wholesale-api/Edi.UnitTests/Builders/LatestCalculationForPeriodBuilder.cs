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

using Energinet.DataHub.Wholesale.Edi.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Builders;

public class LatestCalculationForPeriodBuilder
{
    private Instant _startDate = Instant.FromUtc(2024, 1, 1, 0, 0);
    private Instant _endDate = Instant.FromUtc(2024, 1, 31, 0, 0);
    private Guid _calculationId = Guid.NewGuid();
    private long _version;

    private LatestCalculationForPeriodBuilder()
    {
    }

    public static LatestCalculationForPeriodBuilder LatestCalculationForPeriod()
    {
        return new LatestCalculationForPeriodBuilder();
    }

    public LatestCalculationForPeriod Build()
    {
        return new LatestCalculationForPeriod(
            _startDate,
            _endDate,
            _calculationId,
            _version);
    }

    public LatestCalculationForPeriodBuilder ForPeriod(Instant periodStart, Instant periodEnd)
    {
        _startDate = periodStart;
        _endDate = periodEnd;
        return this;
    }

    public LatestCalculationForPeriodBuilder WithVersion(int version)
    {
        _version = version;
        return this;
    }

    public LatestCalculationForPeriodBuilder WithCalculationId(Guid calculationId)
    {
        _calculationId = calculationId;
        return this;
    }
}

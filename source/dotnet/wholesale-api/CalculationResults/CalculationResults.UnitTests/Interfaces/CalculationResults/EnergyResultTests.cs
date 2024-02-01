﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Interfaces.CalculationResults;

public class EnergyResultTests
{
    [Theory]
    [InlineAutoMoqData]
    public void Ctor_WhenNoPoints_ThrowsArgumentException(
        Guid anyId,
        Guid anyBatchId,
        string anyGridArea,
        string anyFromGridArea,
        TimeSeriesType anyTimeSeriesType,
        CalculationType anyCalculationType,
        Instant anyPeriodStart,
        Instant anyPeriodEnd,
        string anyMeteringPointId,
        long anyVersion)
    {
        // Arrange
        var emptyTimeSeriesPoints = Array.Empty<EnergyTimeSeriesPoint>();

        // Act
        var act = () => new EnergyResult(
            anyId,
            anyBatchId,
            anyGridArea,
            anyTimeSeriesType,
            null,
            null,
            emptyTimeSeriesPoints,
            anyCalculationType,
            anyPeriodStart,
            anyPeriodEnd,
            anyFromGridArea,
            anyMeteringPointId,
            anyVersion);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }
}

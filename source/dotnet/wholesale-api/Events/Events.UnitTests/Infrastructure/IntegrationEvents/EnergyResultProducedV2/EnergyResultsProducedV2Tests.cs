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

using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.Common;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Xunit;
using EnergyResultProduced = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.EnergyResultProducedV2;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.EnergyResultProducedV2;

public class EnergyResultsProducedV2Tests
{
    [Fact]
    public void LargestCalculationResult_FitsInServiceBusMessage()
    {
        var largestRsm014Result = new EnergyResultProduced
        {
            Resolution = EnergyResultProduced.Types.Resolution.Quarter,
            CalculationId = Guid.NewGuid().ToString(),
            CalculationType = EnergyResultProduced.Types.CalculationType.Aggregation,
            QuantityUnit = EnergyResultProduced.Types.QuantityUnit.Kwh,
            AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea = new EnergyResultProduced.Types.AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea
            {
                GridAreaCode = "543",
                EnergySupplierId = "1234567890123456",
                BalanceResponsibleId = "1234567890123456",
            },
            PeriodStartUtc = new Timestamp(),
            PeriodEndUtc = new Timestamp(),
            TimeSeriesType = EnergyResultProduced.Types.TimeSeriesType.Production,
        };

        // 1 month (max 31 days) * 24 hours * 4 quarters
        var maxNumOfPoints = 31 * 24 * 4;
        for (var i = 0; i < maxNumOfPoints; i++)
        {
            var timeSeriesPoint = new EnergyResultProduced.Types.TimeSeriesPoint
            {
                Time = new Timestamp(),
                Quantity = new DecimalValue { Nanos = 123456789, Units = 123456 },
            };
            timeSeriesPoint.QuantityQualities.Add(EnergyResultProduced.Types.QuantityQuality.Measured);
            largestRsm014Result.TimeSeriesPoints.Add(timeSeriesPoint);
        }

        var actualSizeInBytes = largestRsm014Result.CalculateSize();
        var serviceBusMessageSizeLimit = 256000;

        // Assert: Max message size is less than half the size allowed by ServiceBus in order to leave some room for future message size growth
        actualSizeInBytes.Should().BeLessThan(serviceBusMessageSizeLimit / 2);
    }
}

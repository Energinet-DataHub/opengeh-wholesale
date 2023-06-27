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

using Energinet.DataHub.Wholesale.Contracts.Events;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents;

public class CalculationResultCompletedTests
{
    [Fact]
    public void LargestCalculationResult_FitsInServiceBusMessage()
    {
        var largestRsm014Result = new CalculationResultCompleted()
        {
            Resolution = Resolution.Quarter,
            BatchId = Guid.NewGuid().ToString(),
            ProcessType = ProcessType.Aggregation,
            QuantityUnit = QuantityUnit.Kwh,
            AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea = new AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea
            {
                GridAreaCode = "543",
                EnergySupplierGlnOrEic = "1234567890123456",
                BalanceResponsiblePartyGlnOrEic = "1234567890123456",
            },
            PeriodStartUtc = new Timestamp(),
            PeriodEndUtc = new Timestamp(),
            TimeSeriesType = TimeSeriesType.Production,
        };

        // 1 month (max 31 days) * 24 hours * 4 quarters
        var maxNumOfPoints = 31 * 24 * 4;
        for (int i = 0; i < maxNumOfPoints; i++)
        {
            largestRsm014Result.TimeSeriesPoints.Add(new TimeSeriesPoint
            {
                Time = new Timestamp(),
                Quantity = new DecimalValue
                {
                    Nanos = 123456789,
                    Units = 123456,
                },
                QuantityQuality = QuantityQuality.Measured,
            });
        }

        var actualSizeInBytes = largestRsm014Result.CalculateSize();
        var serviceBusMessageSizeLimit = 256000;

        // Assert: Max message size is less than half the size allowed by ServiceBus in order to leave some room for future message size growth
        actualSizeInBytes.Should().BeLessThan(serviceBusMessageSizeLimit / 2);
    }
}

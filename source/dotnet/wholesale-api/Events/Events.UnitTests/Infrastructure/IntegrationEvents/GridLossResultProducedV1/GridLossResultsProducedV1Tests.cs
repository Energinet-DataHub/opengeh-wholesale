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

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.GridLossResultProducedV1;

public class GridLossResultsProducedV1Tests
{
    [Fact]
    public void LargestCalculationResult_FitsInServiceBusMessage()
    {
        var largestRsm014Result = new Contracts.IntegrationEvents.GridLossResultProducedV1
        {
            Resolution = Contracts.IntegrationEvents.GridLossResultProducedV1.Types.Resolution.Quarter,
            CalculationId = Guid.NewGuid().ToString(),
            QuantityUnit = Contracts.IntegrationEvents.GridLossResultProducedV1.Types.QuantityUnit.Kwh,
            PeriodStartUtc = new Timestamp(),
            PeriodEndUtc = new Timestamp(),
        };

        // 1 month (max 31 days) * 24 hours * 4 quarters
        var maxNumOfPoints = 31 * 24 * 4;
        for (var i = 0; i < maxNumOfPoints; i++)
        {
            var timeSeriesPoint = new Contracts.IntegrationEvents.GridLossResultProducedV1.Types.TimeSeriesPoint
            {
                Time = new Timestamp(),
                Quantity = new DecimalValue { Nanos = 123456789, Units = 123456 },
            };
            largestRsm014Result.TimeSeriesPoints.Add(timeSeriesPoint);
        }

        var actualSizeInBytes = largestRsm014Result.CalculateSize();
        var serviceBusMessageSizeLimit = 256000;

        // Assert: Max message size is less than half the size allowed by ServiceBus in order to leave some room for future message size growth
        actualSizeInBytes.Should().BeLessThan(serviceBusMessageSizeLimit / 2);
    }
}

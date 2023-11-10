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

using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Common;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.Types
{
    public class InstantExtensionsTests
    {
        [Theory]
        [InlineData(2021, 10, 12, 14, 27, 59)]
        public void IsEndDefault_ReturnsExpectedResult(int year, int month, int day, int hour, int minute, int second)
        {
            var instant = Instant.FromUtc(year, month, day, hour, minute, second);

            var actual = instant.ToTimestamp();

            actual.ToDateTimeOffset().Should().Be(instant.ToDateTimeOffset());
        }
    }
}

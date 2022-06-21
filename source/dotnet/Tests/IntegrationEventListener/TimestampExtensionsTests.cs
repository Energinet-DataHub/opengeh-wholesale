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

using Energinet.DataHub.Wholesale.IntegrationEventListener.Extensions;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.Tests.IntegrationEventListener;

public class TimestampExtensionsTests
{
    [Fact]
    public void ToInstant_IncomingProtoBuf_ConvertedToInstant()
    {
        // Arrange
        var expected = new DateTimeOffset(
            2022,
            11,
            12,
            1,
            2,
            3,
            50,
            TimeSpan.Zero);

        var target = Timestamp.FromDateTimeOffset(expected);

        // Act
        var actual = target.ToInstant();

        // Assert
        actual.Should().Be(Instant.FromDateTimeOffset(expected));
    }
}

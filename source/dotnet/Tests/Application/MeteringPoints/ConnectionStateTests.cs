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

using Energinet.DataHub.Wholesale.IntegrationEventListener.MeteringPoints;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Tests.Application.MeteringPoints;

public class ConnectionStateTests
{
    [Theory]
    [InlineData(0, nameof(ConnectionState.Unknown))]
    [InlineData(1, nameof(ConnectionState.New))]
    [InlineData(2, nameof(ConnectionState.Connected))]
    [InlineData(3, nameof(ConnectionState.Disconnected))]
    [InlineData(4, nameof(ConnectionState.ClosedDown))]
    public void Value_IsDefinedCorrectly(int expected, string name)
    {
        // Arrange
        var actual = Enum.Parse<ConnectionState>(name);

        // Assert
        actual.Should().Be((ConnectionState)expected);
    }
}

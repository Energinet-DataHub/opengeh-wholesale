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

using Azure;
using Azure.Storage.Files.DataLake;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.WebApi.HealthChecks.DataLake;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.WebApi.HealthChecks.DataLake;

public class DataLakeHealthCheckTests
{
    [Theory]
    [InlineAutoMoqData(6, 20, 14, false, HealthStatus.Unhealthy)] // Within hour interval but should be Unhealthy because check failed
    [InlineAutoMoqData(6, 20, 14, true, HealthStatus.Healthy)] // Within hour interval should be Healthy because check was successful
    [InlineAutoMoqData(15, 20, 14, true, HealthStatus.Healthy)] // Healthy because outside check interval - hours 15-20
    [InlineAutoMoqData(14, 14, 14, true, HealthStatus.Healthy)] // Healthy because just inside check interval and check was successful
    public async Task DataLakeHealthCheckTick_When_Calling_Dependency_Returns_HealthStatus(
        int startHour,
        int endHour,
        int currentHour,
        bool checkStatus,
        HealthStatus expectedHealthStatus,
        Mock<DataLakeFileSystemClient> dataLakeFileSystemClientMock,
        Mock<IClock> clock)
    {
        // Arrange
        var options = new DataLakeOptions
        {
            DATALAKE_HEALTH_CHECK_START = new TimeOnly(startHour, 0),
            DATALAKE_HEALTH_CHECK_END = new TimeOnly(endHour, 0),
        };
        clock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2021, 1, 1, currentHour, 0));
        dataLakeFileSystemClientMock.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(checkStatus, default!));
        var sut = new DataLakeHealthRegistration(dataLakeFileSystemClientMock.Object, clock.Object, options);

        // Act
        var actualHealthStatus = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        // Assert
        actualHealthStatus.Status.Should().Be(expectedHealthStatus);
    }
}

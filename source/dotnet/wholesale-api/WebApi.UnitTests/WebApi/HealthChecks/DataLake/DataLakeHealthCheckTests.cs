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
using Energinet.DataHub.Wholesale.Common.Infrastructure.HealthChecks.DataLake;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.WebApi.HealthChecks.DataLake;

public class DataLakeHealthCheckTests
{
    [Theory]
    [InlineAutoMoqData(false, HealthStatus.Unhealthy)]
    [InlineAutoMoqData(true, HealthStatus.Healthy)]
    public async Task DataLakeHealthCheckTick_When_Calling_Dependency_Returns_HealthStatus(
        bool dataLakeExists,
        HealthStatus expectedHealthStatus,
        Mock<DataLakeFileSystemClient> dataLakeFileSystemClientMock,
        Mock<IClock> clock)
    {
        // Arrange
        dataLakeFileSystemClientMock
            .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(dataLakeExists, default!));
        var sut = new DataLakeHealthRegistration(dataLakeFileSystemClientMock.Object, clock.Object, new DataLakeOptions());

        // Act
        var actualHealthStatus = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        // Assert
        actualHealthStatus.Status.Should().Be(expectedHealthStatus);
    }
}

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

using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Infrastructure;
using Energinet.DataHub.Wholesale.Infrastructure.Integration.DataLake;
using Energinet.DataHub.Wholesale.Infrastructure.Processes;
using Energinet.DataHub.Wholesale.WebApi.UnitTests.Infrastructure.SettlementReport;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Categories;
using DataLakeFileClient = Azure.Storage.Files.DataLake.DataLakeFileClient;
using TimeSeriesType = Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Infrastructure.Processes;

[UnitTest]
public class ProcessStepResultRepositoryTests
{
    [Theory]
    [AutoMoqData]
    public async Task GetAsync_ReturnsProcessActorResult(
        [Frozen] Mock<IJsonNewlineSerializer> jsonNewlineSerializerMock,
        [Frozen] Mock<DataLakeFileClient> dataLakeFileClientMock,
        [Frozen] Mock<IDataLakeClient> dataLakeClientMock)
    {
        // Arrange
        var stream = new Mock<Stream>();

        dataLakeFileClientMock
            .Setup(x => x.OpenReadAsync(It.IsAny<bool>(), It.IsAny<long>(), It.IsAny<int?>(), default))
            .ReturnsAsync(stream.Object);
        dataLakeClientMock.Setup(x => x.GetDataLakeFileClientAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(dataLakeFileClientMock.Object);
        var processResultPoint = new ProcessResultPoint("1.00", "measured", "2022-05-31T22:00:00");
        jsonNewlineSerializerMock.Setup(x => x.DeserializeAsync<ProcessResultPoint>(stream.Object))
            .ReturnsAsync(new List<ProcessResultPoint>
            {
                processResultPoint,
            });

        var sut = new ProcessStepResultRepository(
            dataLakeClientMock.Object,
            jsonNewlineSerializerMock.Object);

        // Act
        var actual = await sut.GetAsync(Guid.NewGuid(), new GridAreaCode("123"), TimeSeriesType.Production, null, null);

        // Assert
        actual.Should().NotBeNull();
    }

    [Fact]
    public static async Task GetDirectoryForTotalGridAreaGrouping_MatchesContract()
    {
        // Arrange
        const string batchId = "eac4a18d-ed5f-46ba-bfe7-435ec0323519";
        const string gridAreaCode = "123";
        var calculationFilePathsContract = await CalculationFilePathsContract.GetAsync();
        var expected = calculationFilePathsContract.ResultFileForTotalGridArea;

        // Act
        var actual = ProcessStepResultRepository.GetDirectoryForTotalGridArea(new Guid(batchId), new GridAreaCode(gridAreaCode), TimeSeriesType.Production);

        // Assert
        actual.Should().MatchRegex(expected.DirectoryExpression);
    }

    [Fact]
    public static async Task GetDirectoryForEsGridAreaGrouping_MatchesContract()
    {
        // Arrange
        const string batchId = "eac4a18d-ed5f-46ba-bfe7-435ec0323519";
        const string gridAreaCode = "123";
        var calculationFilePathsContract = await CalculationFilePathsContract.GetAsync();
        var expected = calculationFilePathsContract.ResultFile;

        // Act
        var actual = ProcessStepResultRepository.GetDirectoryForEsGridArea(new Guid(batchId), new GridAreaCode(gridAreaCode), TimeSeriesType.Production, "energySupplierGln");

        // Assert
        actual.Should().MatchRegex(expected.DirectoryExpression);
    }

    [Fact]
    public static async Task GetDirectoryForEsBrpGridAreaGrouping_MatchesContract()
    {
        // Arrange
        const string batchId = "eac4a18d-ed5f-46ba-bfe7-435ec0323519";
        const string gridAreaCode = "123";
        var calculationFilePathsContract = await CalculationFilePathsContract.GetAsync();
        var expected = calculationFilePathsContract.ResultFileForGaBrpEs;

        // Act
        var actual = ProcessStepResultRepository.GetDirectoryForEsBrpGridArea(new Guid(batchId), new GridAreaCode(gridAreaCode), TimeSeriesType.Production, "balanceResponsiblePartyGln", "energySupplierGln");

        // Assert
        actual.Should().MatchRegex(expected.DirectoryExpression);
    }

    [Theory]
    [InlineData(TimeSeriesType.NonProfiledConsumption, "non_profiled_consumption")]
    [InlineData(TimeSeriesType.FlexConsumption, "consumption")]
    [InlineData(TimeSeriesType.Production, "production")]
    public void GetResultFileSpecification_DirectoryContainsCorrectlyMappedTimeSeriesTypeString(TimeSeriesType timeSeriesType, string expectedTimeSeriesType)
    {
        // Arrange
        const string batchId = "eac4a18d-ed5f-46ba-bfe7-435ec0323519";
        const string gridAreaCode = "123";

        // Act
        var actual = ProcessStepResultRepository.GetDirectoryForTotalGridArea(new Guid(batchId), new GridAreaCode(gridAreaCode), timeSeriesType);

        // Assert
        actual.Should().Contain(expectedTimeSeriesType);
    }
}

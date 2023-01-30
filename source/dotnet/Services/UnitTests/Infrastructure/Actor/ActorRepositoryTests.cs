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
using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Domain.ActorAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.BatchActor;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.DataLake;
using Energinet.DataHub.Wholesale.Tests.Infrastructure.SettlementReport;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Categories;
using TimeSeriesType = Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Tests.Infrastructure.Actor;

[UnitTest]
public class ActorRepositoryTests
{
    [Theory]
    [AutoMoqData]
    public async Task GetAsync_ReturnsBatchActor(
        [Frozen] Mock<IJsonNewlineSerializer> dataLakeTypeFactoryMock,
        [Frozen] Mock<DataLakeFileSystemClient> dataLakeFileSystemClientMock,
        [Frozen] Mock<DataLakeDirectoryClient> dataLakeDirectoryClientMock,
        [Frozen] Mock<DataLakeFileClient> dataLakeFileClientMock,
        [Frozen] Mock<Response<bool>> responseMock)
    {
        // Arrange
        const string pathWithKnownExtension = "my_file.json";
        var asyncPageable = CreateAsyncPageableWithOnePathItem(pathWithKnownExtension);
        var stream = new Mock<Stream>();

        dataLakeDirectoryClientMock
            .Setup(client => client.GetPathsAsync(false, false, It.IsAny<CancellationToken>()))
            .Returns(asyncPageable);
        responseMock.Setup(res => res.Value).Returns(true);
        dataLakeDirectoryClientMock.Setup(dirClient => dirClient.ExistsAsync(default))
            .ReturnsAsync(responseMock.Object);
        dataLakeFileSystemClientMock.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
            .Returns(dataLakeDirectoryClientMock.Object);
        dataLakeFileSystemClientMock.Setup(x => x.GetFileClient(pathWithKnownExtension))
            .Returns(dataLakeFileClientMock.Object);
        dataLakeFileClientMock
            .Setup(x => x.OpenReadAsync(It.IsAny<bool>(), It.IsAny<long>(), It.IsAny<int?>(), default))
            .ReturnsAsync(stream.Object);
        var batchActor = new Wholesale.Domain.ActorAggregate.Actor("AnyGln");
        dataLakeTypeFactoryMock.Setup(x => x.DeserializeAsync<Wholesale.Domain.ActorAggregate.Actor>(stream.Object))
            .ReturnsAsync(new List<Wholesale.Domain.ActorAggregate.Actor>
            {
                batchActor,
            });

        var sut = new ActorRepository(
            dataLakeFileSystemClientMock.Object,
            dataLakeTypeFactoryMock.Object);

        // Act
        var actual = await sut.GetAsync(Guid.NewGuid(), new GridAreaCode("123"), TimeSeriesType.Production, MarketRole.EnergySupplier);

        // Assert
        actual.Should().NotBeNull();
    }

    [Theory]
    [InlineData(TimeSeriesType.NonProfiledConsumption)]
    [InlineData(TimeSeriesType.FlexConsumption)]
    [InlineData(TimeSeriesType.Production)]
    public static async Task GetActorFileSpecification_MatchesContract(TimeSeriesType timeSeriesType)
    {
        // Arrange
        const string batchId = "eac4a18d-ed5f-46ba-bfe7-435ec0323519";
        const string gridAreaCode = "123";
        var calculationFilePathsContract = await CalculationFilePathsContract.GetAsync();
        var expected = calculationFilePathsContract.ActorsFile;

        // Act
        var actual = ActorRepository.GetActorListFileSpecification(new Guid(batchId), new GridAreaCode(gridAreaCode), timeSeriesType, MarketRole.EnergySupplier);

        // Assert
        actual.Extension.Should().Be(expected.Extension);
        actual.Directory.Should().MatchRegex(expected.DirectoryExpression);
    }

    private static AsyncPageable<PathItem> CreateAsyncPageableWithOnePathItem(string path)
    {
        var pathItem = DataLakeModelFactory
            .PathItem(path, false, DateTimeOffset.Now, ETag.All, 1, "owner", "group", "permissions");
        var page = Page<PathItem>.FromValues(new[] { pathItem }, null, Mock.Of<Response>());
        var asyncPageable = AsyncPageable<PathItem>.FromPages(new[] { page });
        return asyncPageable;
    }
}

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
using Energinet.DataHub.Wholesale.Domain.ActorAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Infrastructure;
using Energinet.DataHub.Wholesale.Infrastructure.BatchActor;
using Energinet.DataHub.Wholesale.Infrastructure.Integration.DataLake;
using Energinet.DataHub.Wholesale.WebApi.UnitTests.Infrastructure.SettlementReport;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Categories;
using DataLakeFileClient = Azure.Storage.Files.DataLake.DataLakeFileClient;
using TimeSeriesType = Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Infrastructure.Actor;

[UnitTest]
public class ActorRepositoryTests
{
    [Theory]
    [AutoMoqData]
    public async Task GetAsync_ReturnsBatchActor(
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

        jsonNewlineSerializerMock.Setup(x => x.DeserializeAsync<Energinet.DataHub.Wholesale.Infrastructure.BatchActor.ActorRelation>(stream.Object))
            .ReturnsAsync(new List<Energinet.DataHub.Wholesale.Infrastructure.BatchActor.ActorRelation>());

        var sut = new ActorRepository(
            dataLakeClientMock.Object,
            jsonNewlineSerializerMock.Object);

        // Act
        var actual = await sut.GetEnergySuppliersAsync(Guid.NewGuid(), new GridAreaCode("123"), TimeSeriesType.Production);

        // Assert
        actual.Should().NotBeNull();
    }

    [Theory]
    [AutoMoqData]
    public async Task GetAsync_WhenDuplicateEnergySuppliers_ReturnsDistinctActorsList(
        [Frozen] Mock<IJsonNewlineSerializer> jsonNewlineSerializerMock,
        [Frozen] Mock<DataLakeFileClient> dataLakeFileClientMock,
        [Frozen] Mock<IDataLakeClient> dataLakeClientMock)
    {
        // Arrange
        var actorRelationsDeserialized = new List<Energinet.DataHub.Wholesale.Infrastructure.BatchActor.ActorRelation>
        {
            new Energinet.DataHub.Wholesale.Infrastructure.BatchActor.ActorRelation("123", "111"),
            new Energinet.DataHub.Wholesale.Infrastructure.BatchActor.ActorRelation("234", "222"),
            new Energinet.DataHub.Wholesale.Infrastructure.BatchActor.ActorRelation("123", "333"),
        };
        var expectedGln = new List<string>() { "123", "234" }; // distinct gln list

        dataLakeFileClientMock
            .Setup(x => x.OpenReadAsync(It.IsAny<bool>(), It.IsAny<long>(), It.IsAny<int?>(), default))
            .ReturnsAsync(It.IsAny<Stream>());
        dataLakeClientMock.Setup(x => x.GetDataLakeFileClientAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(dataLakeFileClientMock.Object);
        jsonNewlineSerializerMock.Setup(x => x.DeserializeAsync<Energinet.DataHub.Wholesale.Infrastructure.BatchActor.ActorRelation>(It.IsAny<Stream>()))
            .ReturnsAsync(actorRelationsDeserialized);

        var sut = new ActorRepository(
            dataLakeClientMock.Object,
            jsonNewlineSerializerMock.Object);

        // Act
        var actual = await sut.GetEnergySuppliersAsync(Guid.NewGuid(), new GridAreaCode("123"), TimeSeriesType.Production);

        // Assert
        actual.Should().BeEquivalentTo(expectedGln);
    }

    [Theory]
    [AutoMoqData]
    public async Task GetAsync_WhenDuplicateBalanceResponsibleParies_ReturnsDistinctActorsList(
        [Frozen] Mock<IJsonNewlineSerializer> jsonNewlineSerializerMock,
        [Frozen] Mock<DataLakeFileClient> dataLakeFileClientMock,
        [Frozen] Mock<IDataLakeClient> dataLakeClientMock)
    {
        // Arrange
        var actorRelationsDeserialized = new List<Energinet.DataHub.Wholesale.Infrastructure.BatchActor.ActorRelation>
        {
            new Energinet.DataHub.Wholesale.Infrastructure.BatchActor.ActorRelation("123", "111"),
            new Energinet.DataHub.Wholesale.Infrastructure.BatchActor.ActorRelation("234", "111"),
            new Energinet.DataHub.Wholesale.Infrastructure.BatchActor.ActorRelation("345", "333"),
        };
        var expectedGln = new List<string>() { "111", "333" }; // distinct gln list

        dataLakeFileClientMock
            .Setup(x => x.OpenReadAsync(It.IsAny<bool>(), It.IsAny<long>(), It.IsAny<int?>(), default))
            .ReturnsAsync(It.IsAny<Stream>());
        dataLakeClientMock.Setup(x => x.GetDataLakeFileClientAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(dataLakeFileClientMock.Object);
        jsonNewlineSerializerMock.Setup(x => x.DeserializeAsync<Energinet.DataHub.Wholesale.Infrastructure.BatchActor.ActorRelation>(It.IsAny<Stream>()))
            .ReturnsAsync(actorRelationsDeserialized);

        var sut = new ActorRepository(
            dataLakeClientMock.Object,
            jsonNewlineSerializerMock.Object);

        // Act
        var actual = await sut.GetBalanceResponsiblePartiesAsync(Guid.NewGuid(), new GridAreaCode("123"), TimeSeriesType.Production);

        // Assert
        actual.Should().BeEquivalentTo(expectedGln);
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
        var (directory, extension) = ActorRepository.GetActorListFileSpecification(new Guid(batchId), new GridAreaCode(gridAreaCode), timeSeriesType);

        // Assert
        extension.Should().Be(expected.Extension);
        directory.Should().MatchRegex(expected.DirectoryExpression);
    }
}

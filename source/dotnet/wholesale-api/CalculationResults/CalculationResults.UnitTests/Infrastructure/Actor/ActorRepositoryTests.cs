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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.BatchActor;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.DataLake;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.JsonNewlineSerializer;
using Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SettlementReport;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Categories;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.Actor;

[UnitTest]
public class ActorRepositoryTests
{
    [Theory]
    [AutoMoqData]
    public async Task GetEnergySuppliersAsync_ReturnsBatchActor(
        [Frozen] Mock<IJsonNewlineSerializer> jsonNewlineSerializerMock,
        [Frozen] Mock<IDataLakeClient> dataLakeClientMock)
    {
        // Arrange
        MockSetup(jsonNewlineSerializerMock, dataLakeClientMock, new List<ActorRelation>());

        var sut = new ActorClient(dataLakeClientMock.Object, jsonNewlineSerializerMock.Object);

        // Act
        var actual = await sut.GetEnergySuppliersAsync(Guid.NewGuid(), "123", TimeSeriesType.Production);

        // Assert
        actual.Should().NotBeNull();
    }

    [Theory]
    [AutoMoqData]
    public async Task GetEnergySuppliersAsync_WhenDuplicateEnergySuppliers_ReturnsDistinctActorsList(
        [Frozen] Mock<IJsonNewlineSerializer> jsonNewlineSerializerMock,
        [Frozen] Mock<IDataLakeClient> dataLakeClientMock)
    {
        // Arrange
        var actorRelationsDeserialized = new List<ActorRelation>
        {
            new("123", "111"),
            new("234", "222"),
            new("123", "333"),
        };

        var expectedGln = new List<Interfaces.ActorClient.Model.Actor> { new("123"), new("234") }; // distinct gln list

        MockSetup(jsonNewlineSerializerMock, dataLakeClientMock, actorRelationsDeserialized);

        var sut = new ActorClient(
            dataLakeClientMock.Object,
            jsonNewlineSerializerMock.Object);

        // Act
        var actual = await sut.GetEnergySuppliersAsync(Guid.NewGuid(), "123", TimeSeriesType.Production);

        // Assert
        actual.Should().BeEquivalentTo(expectedGln);
    }

    [Theory]
    [AutoMoqData]
    public async Task GetBalanceResponsiblePartiesAsync_WhenDuplicateBalanceResponsibleParties_ReturnsDistinctActorsList(
        [Frozen] Mock<IJsonNewlineSerializer> jsonNewlineSerializerMock,
        [Frozen] Mock<IDataLakeClient> dataLakeClientMock)
    {
        // Arrange
        var actorRelationsDeserialized = new List<ActorRelation>
        {
            new("123", "111"),
            new("234", "111"),
            new("345", "333"),
        };

        var expectedGln = new List<Interfaces.ActorClient.Model.Actor> { new("111"), new("333") }; // distinct gln list

        MockSetup(jsonNewlineSerializerMock, dataLakeClientMock, actorRelationsDeserialized);

        var sut = new ActorClient(
            dataLakeClientMock.Object,
            jsonNewlineSerializerMock.Object);

        // Act
        var actual = await sut.GetBalanceResponsiblePartiesAsync(Guid.NewGuid(), "123", TimeSeriesType.Production);

        // Assert
        actual.Should().BeEquivalentTo(expectedGln);
    }

    [Theory]
    [AutoMoqData]
    public async Task GetEnergySuppliersByBalanceResponsiblePartyAsync_ReturnsExpectedActors(
        [Frozen] Mock<IJsonNewlineSerializer> jsonNewlineSerializerMock,
        [Frozen] Mock<IDataLakeClient> dataLakeClientMock)
    {
        // Arrange
        const string targetBrpGln = "TargetBrpGln";
        const string otherBrpGln = "OtherGln";
        const string targetEs1Gln = "TargetEs1Gln";
        const string targetEs2Gln = "TargetEs2Gln";
        var actorRelationsDeserialized = new List<ActorRelation>
        {
            new(targetEs1Gln, targetBrpGln),
            new("123", otherBrpGln),
            new(targetEs2Gln, targetBrpGln),
        };

        var expectedGln = new List<Interfaces.ActorClient.Model.Actor> { new(targetEs1Gln), new(targetEs2Gln) };

        MockSetup(jsonNewlineSerializerMock, dataLakeClientMock, actorRelationsDeserialized);

        var sut = new ActorClient(
            dataLakeClientMock.Object,
            jsonNewlineSerializerMock.Object);

        // Act
        var actual = await sut.GetEnergySuppliersByBalanceResponsiblePartyAsync(Guid.NewGuid(), "123", TimeSeriesType.Production, targetBrpGln);

        // Assert
        actual.Should().BeEquivalentTo(expectedGln);
    }

    [Theory]
    [AutoMoqData]
    public async Task GetEnergySuppliersByBalanceResponsiblePartyAsync_WhenNoMatchingBrp_ReturnsEmptyList(
        [Frozen] Mock<IJsonNewlineSerializer> jsonNewlineSerializerMock,
        [Frozen] Mock<IDataLakeClient> dataLakeClientMock)
    {
        // Arrange
        const string targetBrpGln = "TargetBrpGln";
        const string otherBrpGln = "OtherGln";
        var actorRelationsDeserialized = new List<ActorRelation>
        {
            new("123", otherBrpGln),
            new("234", otherBrpGln),
            new("345", otherBrpGln),
        };

        MockSetup(jsonNewlineSerializerMock, dataLakeClientMock, actorRelationsDeserialized);

        var sut = new ActorClient(
            dataLakeClientMock.Object,
            jsonNewlineSerializerMock.Object);

        // Act
        var actual = await sut.GetEnergySuppliersByBalanceResponsiblePartyAsync(Guid.NewGuid(), "123", TimeSeriesType.Production, targetBrpGln);

        // Assert
        actual.Length.Should().Be(0);
    }

    [Theory]
    [AutoMoqData]
    public async Task GetEnergySuppliersByBalanceResponsiblePartyAsync_WhenEsHasTwoBrps_ReturnsEsForBothBrps(
        [Frozen] Mock<IJsonNewlineSerializer> jsonNewlineSerializerMock,
        [Frozen] Mock<IDataLakeClient> dataLakeClientMock)
    {
        // Arrange
        const string brp1Gln = "Brp1Gln";
        const string brp2Gln = "Brp2Gln";
        const string energySupplierGln = "EsGln";
        var actorRelationsDeserialized = new List<ActorRelation>
        {
            new(energySupplierGln, brp1Gln),
            new(energySupplierGln, brp2Gln),
        };

        var expectedGln = new List<Interfaces.ActorClient.Model.Actor> { new(energySupplierGln) };

        MockSetup(jsonNewlineSerializerMock, dataLakeClientMock, actorRelationsDeserialized);

        var sut = new ActorClient(
            dataLakeClientMock.Object,
            jsonNewlineSerializerMock.Object);

        // Act
        var actual1 = await sut.GetEnergySuppliersByBalanceResponsiblePartyAsync(Guid.NewGuid(), "123", TimeSeriesType.Production, brp1Gln);
        var actual2 = await sut.GetEnergySuppliersByBalanceResponsiblePartyAsync(Guid.NewGuid(), "123", TimeSeriesType.Production, brp2Gln);

        // Assert
        actual1.Should().BeEquivalentTo(expectedGln);
        actual2.Should().BeEquivalentTo(expectedGln);
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
        var (directory, extension) = ActorClient.GetActorListFileSpecification(new Guid(batchId), gridAreaCode, timeSeriesType);

        // Assert
        extension.Should().Be(expected.Extension);
        directory.Should().MatchRegex(expected.DirectoryExpression);
    }

    [Theory]
    [AutoMoqData]
    public async Task GetEnergySuppliersAsync_WhenActorRelationsPropsIsNull_ActorRelationIsRemoved(
        [Frozen] Mock<IJsonNewlineSerializer> jsonNewlineSerializerMock,
        [Frozen] Mock<IDataLakeClient> dataLakeClientMock)
    {
        // Arrange
        MockSetup(jsonNewlineSerializerMock, dataLakeClientMock, new List<ActorRelation>
        {
            new(null!, null!),
        });

        var sut = new ActorClient(dataLakeClientMock.Object, jsonNewlineSerializerMock.Object);

        // Act
        var actual = await sut.GetEnergySuppliersAsync(Guid.NewGuid(), "123", TimeSeriesType.Production);

        // Assert
        actual.Should().BeEmpty();
    }

    private static void MockSetup(Mock<IJsonNewlineSerializer> jsonNewlineSerializerMock, Mock<IDataLakeClient> dataLakeClientMock, List<ActorRelation> actorRelationsDeserialized)
    {
        dataLakeClientMock.Setup(x => x.FindFileAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(Guid.NewGuid().ToString());
        dataLakeClientMock.Setup(x => x.GetReadableFileStreamAsync(It.IsAny<string>())).ReturnsAsync(Stream.Null);
        jsonNewlineSerializerMock.Setup(x => x.DeserializeAsync<ActorRelation>(It.IsAny<Stream>())).ReturnsAsync(actorRelationsDeserialized);
    }
}

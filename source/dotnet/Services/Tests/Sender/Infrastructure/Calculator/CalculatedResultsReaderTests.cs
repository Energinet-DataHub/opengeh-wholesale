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

using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Application.Infrastructure;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Sender.Infrastructure;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Persistence.Processes;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Services;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Sender.Infrastructure.Calculator;

[UnitTest]
public sealed class CalculatedResultsReaderTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task When_ReadResultIsCalled_ReturnBalanceFixingResultDto(
        Mock<IBatchFileManager> batchFileManagerMock,
        Guid batchId)
    {
        // Arrange
        const int position = 1;
        const string quantity = "0.000";
        const Quality quality = Quality.Incomplete;
        const string gridAreaCode = "123";
        const string streamString = "{\"quantity\":\"0.000\",\"quality\":2,\"position\":1}";

        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        await writer.WriteAsync(streamString);
        await writer.FlushAsync();
        stream.Position = 0;

        var expected = new PointDto(position, quantity, quality);

        var process = new Process(null!, gridAreaCode, batchId);
        batchFileManagerMock.Setup(x => x.GetResultFileStreamAsync(batchId, new GridAreaCode(gridAreaCode))).ReturnsAsync(stream);
        var calculatedResultsReader = new CalculatedResultsReader(new JsonSerializer(), batchFileManagerMock.Object);

        // Act
        var actual = await calculatedResultsReader.ReadResultAsync(process);

        // Assert
        actual.Points.First().position.Should().Be(expected.position);
        actual.Points.First().quantity.Should().Be(expected.quantity);
        actual.Points.First().quality.Should().Be(expected.quality);
    }
}

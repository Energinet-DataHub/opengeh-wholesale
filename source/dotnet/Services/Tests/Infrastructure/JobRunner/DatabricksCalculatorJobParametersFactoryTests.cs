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
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.JobRunner;
using Energinet.DataHub.Wholesale.Tests.TestHelpers;
using FluentAssertions;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.Tests.Infrastructure.JobRunner;

public class DatabricksCalculatorJobParametersFactoryTests
{
    [Theory]
    [InlineAutoMoqData]
    public void CreateParameters_MatchesExpectationOfDatabricksJob(
        [Frozen] Mock<IClock> clockMock,
        DatabricksCalculatorJobParametersFactory sut)
    {
        // Arrange
        clockMock.Setup(clock => clock.GetCurrentInstant()).Returns(Instant.FromUtc(2022, 6, 2, 22, 00));
        var batch = new Batch(
            ProcessType.BalanceFixing,
            new List<GridAreaCode> { new("805"), new("806") },
            Instant.FromUtc(2022, 5, 31, 22, 00),
            Instant.FromUtc(2022, 6, 1, 22, 00),
            clockMock.Object);

        using var stream = EmbeddedResources.GetStream("Infrastructure.JobRunner.calculation-job-parameters-reference.txt");
        using var reader = new StreamReader(stream);

        var expected = reader
            .ReadToEnd()
            .Replace("{batch-id}", batch.Id.ToString())
            .Replace("\r", string.Empty)
            .Split("\n") // Split lines
            .Where(l => !l.StartsWith("#") && l.Length > 0); // Remove empty and comment lines

        // Act
        var actual = sut.CreateParameters(batch);

        // Assert
        actual.Should().Equal(expected);
    }
}

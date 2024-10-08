﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Calculations;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Test.Core;
using FluentAssertions;
using Microsoft.Azure.Databricks.Client.Models;
using NodaTime;
using NodaTime.Extensions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Calculations.UnitTests.Infrastructure.Calculations;

public class DatabricksCalculatorJobParametersFactoryTests
{
    [Theory]
    [InlineAutoMoqData]
    public void CreateParameters_MatchesExpectationOfDatabricksJob(
        DatabricksCalculationParametersFactory sut)
    {
        // Arrange
        var calculation = new Calculation(
            SystemClock.Instance.GetCurrentInstant(),
            CalculationType.BalanceFixing,
            new List<GridAreaCode> { new("805"), new("806"), new("033") },
            DateTimeOffset.Parse("2022-05-31T22:00Z").ToInstant(),
            DateTimeOffset.Parse("2022-06-01T22:00Z").ToInstant(),
            DateTimeOffset.Parse("2022-06-04T22:00Z").ToInstant(),
            DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!,
            new Guid("c2975345-d935-44a2-b7bf-2629db4aa8bf"),
            SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().Ticks,
            false);

        using var stream = EmbeddedResources.GetStream<Root>("DeltaTableContracts.calculation-job-parameters-reference.txt");
        using var reader = new StreamReader(stream);

        var pythonParams = reader
            .ReadToEnd()
            .Replace("{calculation-id}", calculation.Id.ToString())
            .Replace("\r", string.Empty)
            .Split("\n") // Split lines
            .Where(l => !l.StartsWith("#") && l.Length > 0); // Remove empty and comment lines
        var expected = RunParameters.CreatePythonParams(pythonParams);

        // Act
        var actual = sut.CreateParameters(calculation);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineAutoMoqData]
    public void CreateParameters_WhenCalculationIsInternal_MatchesExpectationOfDatabricksJob(
        DatabricksCalculationParametersFactory sut)
    {
        // Arrange
        var calculation = new Calculation(
            SystemClock.Instance.GetCurrentInstant(),
            CalculationType.Aggregation,
            new List<GridAreaCode> { new("805"), new("806"), new("033") },
            DateTimeOffset.Parse("2022-05-31T22:00Z").ToInstant(),
            DateTimeOffset.Parse("2022-06-01T22:00Z").ToInstant(),
            DateTimeOffset.Parse("2022-06-04T22:00Z").ToInstant(),
            DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!,
            new Guid("c2975345-d935-44a2-b7bf-2629db4aa8bf"),
            SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().Ticks,
            true);

        using var stream = EmbeddedResources.GetStream<Root>("DeltaTableContracts.internal-calculation-job-parameters-reference.txt");
        using var reader = new StreamReader(stream);

        var pythonParams = reader
            .ReadToEnd()
            .Replace("{calculation-id}", calculation.Id.ToString())
            .Replace("\r", string.Empty)
            .Split("\n") // Split lines
            .Where(l => !l.StartsWith("#") && l.Length > 0); // Remove empty and comment lines
        var expected = RunParameters.CreatePythonParams(pythonParams);

        // Act
        var actual = sut.CreateParameters(calculation);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }
}

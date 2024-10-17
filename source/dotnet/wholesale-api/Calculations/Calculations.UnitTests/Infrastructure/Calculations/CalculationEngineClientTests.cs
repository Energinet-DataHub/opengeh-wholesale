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

using Energinet.DataHub.Core.Databricks.Jobs.Abstractions;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Calculations;
using FluentAssertions;
using Microsoft.Azure.Databricks.Client.Models;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.Calculations.UnitTests.Infrastructure.Calculations;

public sealed class CalculationEngineClientTests
{
    [Theory]
    [ClassData(typeof(StatusTestCases))]
    public async Task EnsureAllRunLifeCycleStatesAndRunResultStatesAreHandled(
        RunLifeCycleState lifeCycleState,
        RunResultState? resultState)
    {
        // Arrange
        var mockDatabricksCalculatorJobSelector = new Mock<IDatabricksCalculatorJobSelector>();
        var mockClient = new Mock<IJobsApiClient>();
        var mockCalculationParametersFactory = new Mock<ICalculationParametersFactory>();

        mockClient
            .Setup(client => client.Jobs.RunsGet(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (new Run { State = new RunState { LifeCycleState = lifeCycleState, ResultState = resultState, }, },
                    null));

        var sut = new CalculationEngineClient(
            mockDatabricksCalculatorJobSelector.Object,
            mockClient.Object,
            mockCalculationParametersFactory.Object);

        // Act & Assert
        var invokeSut = async () => await sut.GetStatusAsync(new CalculationJobId(1024));
        await invokeSut.Should().NotThrowAsync("all states should be handled");
    }

    private sealed class StatusTestCases : TheoryData<RunLifeCycleState, RunResultState?>
    {
        public StatusTestCases()
        {
            foreach (var lifeCycleState in Enum.GetValues<RunLifeCycleState>())
            {
                if (lifeCycleState == RunLifeCycleState.TERMINATED)
                {
                    foreach (var resultState in Enum.GetValues<RunResultState>())
                    {
                        Add(lifeCycleState, resultState);
                    }
                }

                Add(lifeCycleState, null);
            }
        }
    }
}

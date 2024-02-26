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

using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Attributes;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.LazyFixture;
using Energinet.DataHub.Wholesale.SubsystemTests.Performance.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Performance
{
    [TestCaseOrderer(
        ordererTypeName: "Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Orderers.ScenarioStepOrderer",
        ordererAssemblyName: "Energinet.DataHub.Wholesale.SubsystemTests")]
    public class CalculationJobScenario : SubsystemTestsBase<CalculationJobScenarioFixture>
    {
        public CalculationJobScenario(LazyFixtureFactory<CalculationJobScenarioFixture> lazyFixtureFactory)
            : base(lazyFixtureFactory)
        {
        }

        [ScenarioStep(0)]
        [SubsystemFact]
        public void Given_CalculationJobInput()
        {
            var createdTime = SystemClock.Instance.GetCurrentInstant();
            var createdByUserId = Guid.Parse("DED7734B-DD56-43AD-9EE8-0D7EFDA6C783");
            Fixture.ScenarioState.CalculationJobInput = new Calculation(
                createdTime: createdTime,
                calculationType: Common.Interfaces.Models.CalculationType.Aggregation,
                gridAreaCodes: new List<GridAreaCode> { new GridAreaCode("791") },
                periodStart: Instant.FromDateTimeOffset(new DateTimeOffset(2022, 11, 30, 23, 0, 0, TimeSpan.Zero)),
                periodEnd: Instant.FromDateTimeOffset(new DateTimeOffset(2022, 12, 11, 23, 0, 0, TimeSpan.Zero)),
                executionTimeStart: createdTime, // As long as scheduling is not implemented, execution time start is the same as created time
                dateTimeZone: DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!,
                createdByUserId: createdByUserId,
                version: createdTime.ToDateTimeUtc().Ticks);
        }

        [ScenarioStep(1)]
        [SubsystemFact]
        public async Task When_CalculationJobIsStarted()
        {
            Fixture.ScenarioState.CalculationJobId = await Fixture.StartCalculationJobAsync(Fixture.ScenarioState.CalculationJobInput);

            // Assert
            Fixture.ScenarioState.CalculationJobId.Should().NotBeNull();
        }

        /// <summary>
        /// In this step focus on completing the calculation with a certain 'wait time'.
        /// This is not an exact time for how long it took to perform the calculation,
        /// but the time it took for our retry loop to determine that the calculation has completed.
        /// </summary>
        [ScenarioStep(2)]
        [SubsystemFact]
        public async Task Then_CalculationJobIsCompletedWithinWaitTime()
        {
            var actualWaitResult = await Fixture.WaitForCalculationJobCompletedAsync(
                Fixture.ScenarioState.CalculationJobId,
                waitTimeLimit: TimeSpan.FromMinutes(75));

            Fixture.ScenarioState.Run = actualWaitResult.Run;

            // Assert
            using var assertionScope = new AssertionScope();
            actualWaitResult.IsCompleted.Should().BeTrue();
            actualWaitResult.Run.Should().NotBeNull();
        }

        /// <summary>
        /// In this step we verify the 'duration' of the calculation job is within our 'performance goal'.
        /// This 'duration' is the time we want to reduce during our performance workshop.
        /// </summary>
        [ScenarioStep(3)]
        [SubsystemFact]
        public void AndThen_CalculationJobDurationIsLessThanOrEqualToTimeLimit()
        {
            var calculationJobTimeLimit = TimeSpan.FromMinutes(70);

            // TODO: Verify if this is the correct way to measure the duration. See https://docs.databricks.com/api/azure/workspace/jobs/getrun
            var actualCalculationJobDuration =
                Fixture.ScenarioState.Run.EndTime - Fixture.ScenarioState.Run.StartTime;

            // Assert
            actualCalculationJobDuration.Should().BeGreaterThan(TimeSpan.Zero);
            actualCalculationJobDuration.Should().BeLessThanOrEqualTo(calculationJobTimeLimit);
        }
    }
}

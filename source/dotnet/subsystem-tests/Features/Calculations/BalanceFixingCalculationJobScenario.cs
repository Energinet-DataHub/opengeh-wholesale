using Energinet.DataHub.Core.TestCommon.Xunit.Attributes;
using Energinet.DataHub.Core.TestCommon.Xunit.LazyFixture;
using Energinet.DataHub.Core.TestCommon.Xunit.Orderers;
using Energinet.DataHub.Wholesale.SubsystemTests.Features.Calculations.Fixtures;
using Energinet.DataHub.Wholesale.SubsystemTests.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.Calculations;

[ExecutionContext(AzureEnvironment.AllDev)]
[TestCaseOrderer(
    ordererTypeName: TestCaseOrdererLocation.OrdererTypeName,
    ordererAssemblyName: TestCaseOrdererLocation.OrdererAssemblyName)]
public class BalanceFixingCalculationJobScenario : SubsystemTestsBase<CalculationJobScenarioFixture>
{
    public BalanceFixingCalculationJobScenario(LazyFixtureFactory<CalculationJobScenarioFixture> lazyFixtureFactory)
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
            calculationType: CalculationType.BalanceFixing,
            gridAreaCodes: new List<GridAreaCode> { new("543") },
            periodStart: Instant.FromDateTimeOffset(new DateTimeOffset(2022, 1, 11, 23, 0, 0, TimeSpan.Zero)),
            periodEnd: Instant.FromDateTimeOffset(new DateTimeOffset(2022, 1, 12, 23, 0, 0, TimeSpan.Zero)),
            createdByUserId: createdByUserId,
            false);
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
        var (isCompleted, run) = await Fixture.WaitForCalculationJobCompletedAsync(
            Fixture.ScenarioState.CalculationJobId,
            waitTimeLimit: TimeSpan.FromMinutes(21));

        Fixture.ScenarioState.Run = run;

        // Assert
        using var assertionScope = new AssertionScope();
        isCompleted.Should().BeTrue("because calculation job should complete within time limit.");
        run.Should().NotBeNull();
    }

    /// <summary>
    /// In this step we verify the 'duration' of the calculation job is within our 'performance goal'.
    /// </summary>
    [ScenarioStep(3)]
    [SubsystemFact]
    public void AndThen_CalculationJobDurationIsLessThanOrEqualToTimeLimit()
    {
        var calculationJobTimeLimit = TimeSpan.FromMinutes(18);

        var actualCalculationJobDuration =
            Fixture.ScenarioState.Run.EndTime - Fixture.ScenarioState.Run.StartTime;

        // Assert
        actualCalculationJobDuration.Should().BeGreaterThan(TimeSpan.Zero);
        actualCalculationJobDuration.Should().BeLessThanOrEqualTo(calculationJobTimeLimit);
    }
}

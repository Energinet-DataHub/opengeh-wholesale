﻿using Azure.Monitor.Query;
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
public class WholesaleFixingCalculationJobScenario : SubsystemTestsBase<CalculationJobScenarioFixture>
{
    public WholesaleFixingCalculationJobScenario(LazyFixtureFactory<CalculationJobScenarioFixture> lazyFixtureFactory)
        : base(lazyFixtureFactory)
    {
    }

    [ScenarioStep(0)]
    [SubsystemFact]
    public async Task Given_LatestCalculationVersionBeforeANewCalculationIsStarted()
    {
        var (calculationVersion, message) = await Fixture.GetLatestCalculationVersionFromCalculationsAsync();
        Fixture.ScenarioState.LatestCalculationVersion = calculationVersion;

        // Assert
        calculationVersion.Should().NotBeNull(message);
    }

    [ScenarioStep(1)]
    [SubsystemFact]
    public void AndGiven_CalculationJobInput()
    {
        var createdTime = SystemClock.Instance.GetCurrentInstant();
        var createdByUserId = Guid.Parse("DED7734B-DD56-43AD-9EE8-0D7EFDA6C783");
        Fixture.ScenarioState.CalculationJobInput = new Calculation(
            calculationType: CalculationType.WholesaleFixing,
            gridAreaCodes: new List<GridAreaCode> { new("804") },
            periodStart: Instant.FromDateTimeOffset(new DateTimeOffset(2023, 1, 31, 23, 0, 0, TimeSpan.Zero)),
            periodEnd: Instant.FromDateTimeOffset(new DateTimeOffset(2023, 2, 28, 23, 0, 0, TimeSpan.Zero)),
            createdByUserId: createdByUserId,
            false);
    }

    [ScenarioStep(2)]
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
    [ScenarioStep(3)]
    [SubsystemFact]
    public async Task Then_CalculationJobIsCompletedWithinWaitTime()
    {
        var (isCompleted, run) = await Fixture.WaitForCalculationJobCompletedAsync(
            Fixture.ScenarioState.CalculationJobId,
            waitTimeLimit: TimeSpan.FromMinutes(33));

        Fixture.ScenarioState.Run = run;

        // Assert
        using var assertionScope = new AssertionScope();
        isCompleted.Should().BeTrue("because calculation job should complete within time limit.");
        run.Should().NotBeNull();
    }

    /// <summary>
    /// In this step we verify the 'duration' of the calculation job is within our 'performance goal'.
    /// </summary>
    [ScenarioStep(4)]
    [SubsystemFact]
    public void AndThen_CalculationJobDurationIsLessThanOrEqualToTimeLimit()
    {
        var calculationJobTimeLimit = TimeSpan.FromMinutes(30);

        var actualCalculationJobDuration =
            Fixture.ScenarioState.Run.EndTime - Fixture.ScenarioState.Run.StartTime;

        // Assert
        actualCalculationJobDuration.Should().BeGreaterThan(TimeSpan.Zero);
        actualCalculationJobDuration.Should().BeLessThanOrEqualTo(calculationJobTimeLimit);
    }

    [ScenarioStep(5)]
    [SubsystemFact]
    public async Task AndThen_ACalculationTelemetryLogIsCreated()
    {
        var query = $@"
     AppTraces
     | where AppRoleName == ""dbr-calculation-engine""
     | where SeverityLevel == 1 // Information
     | where Message startswith_cs ""Calculator arguments:""
     | where OperationId != ""00000000000000000000000000000000""
     | where Properties.Subsystem == ""wholesale-aggregations""
     | where Properties.calculation_id == ""{Fixture.ScenarioState.CalculationJobInput.Id}""
     | where Properties.CategoryName == ""Energinet.DataHub.geh_wholesale.calculator_job""
     | count";

        // Assert
        var actual = await Fixture.QueryLogAnalyticsAsync(query, new QueryTimeRange(TimeSpan.FromMinutes(60)));

        using var assertionScope = new AssertionScope();
        actual.Value.Table.Rows[0][0].Should().Be(1); // count == 1
    }

    [ScenarioStep(6)]
    [SubsystemFact]
    public async Task AndThen_ACalculationTelemetryTraceWithASpanIsCreated()
    {
        var query = $@"
     AppDependencies
     | where Target == ""energy""
     | where Name == ""energy""
     | where DependencyType == ""InProc""
     | where Success == true
     | where ResultCode == 0
     | where AppRoleName == ""dbr-calculation-engine""
     | where Properties.Subsystem == ""wholesale-aggregations""
     | where Properties.calculation_id == ""{Fixture.ScenarioState.CalculationJobInput.Id}""
     | count";

        // Assert
        var actual = await Fixture.QueryLogAnalyticsAsync(query, new QueryTimeRange(TimeSpan.FromMinutes(60)));

        using var assertionScope = new AssertionScope();
        actual.Value.Table.Rows[0][0].Should().Be(1); // count == 1
    }

    [ScenarioStep(7)]
    [SubsystemFact]
    public async Task AndThen_OneViewOrTableInEachPublicDataModelMustExistsAndContainData()
    {
        // Arrange
        var publicDataModelsAndTables = new List<(string ModelName, string TableName)>
             {
                 new("wholesale_sap", "energy_v1"),
                 new("wholesale_results", "energy_v1"),
                 new("wholesale_basis_data", "metering_point_periods_v1"),
             };

        // Act
        var results = await Fixture.ArePublicDataModelsAccessibleAsync(publicDataModelsAndTables);

        // Assert
        using var assertionScope = new AssertionScope();
        foreach (var actual in results)
        {
            actual.IsAccessible.Should().Be(true, actual.ErrorMessage);
        }
    }

    [ScenarioStep(8)]
    [SubsystemFact]
    public async Task AndThen_CheckThatIdentityColumnOnCalculationsIsWorkingCorrectly()
    {
        // Arrange
        var previousCalculationVersion = Fixture.ScenarioState.LatestCalculationVersion;

        // Act
        var (calculationVersion, message) = await Fixture.GetCalculationVersionOfCalculationIdFromCalculationsAsync(Fixture.ScenarioState.CalculationJobInput.Id);

        // Assert
        (calculationVersion > previousCalculationVersion).Should().BeTrue(message);
    }
}

using System.Collections.Concurrent;
using Azure;
using Azure.Identity;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Core.TestCommon.Xunit.Extensions;
using Energinet.DataHub.Core.TestCommon.Xunit.LazyFixture;
using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Calculations;
using Energinet.DataHub.Wholesale.SubsystemTests.Features.Calculations.States;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures;
using Microsoft.Azure.Databricks.Client;
using Microsoft.Azure.Databricks.Client.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.Calculations.Fixtures;

public sealed class CalculationJobScenarioFixture : LazyFixtureBase
{
    public CalculationJobScenarioFixture(IMessageSink diagnosticMessageSink)
        : base(diagnosticMessageSink)
    {
        Configuration = new CalculationJobScenarioConfiguration();
        ScenarioState = new CalculationJobScenarioState();
        LogsQueryClient = new LogsQueryClient(new DefaultAzureCredential());
        DatabricksSqlWarehouseQueryExecutor = GetDatabricksSqlWarehouseQueryExecutor();
    }

    public CalculationJobScenarioState ScenarioState { get; }

    private CalculationJobScenarioConfiguration Configuration { get; }

    /// <summary>
    /// The actual client is not created until <see cref="OnInitializeAsync"/> has been called by the base class.
    /// </summary>
    private DatabricksClient DatabricksClient { get; set; } = null!;

    private LogsQueryClient LogsQueryClient { get; }

    private DatabricksSqlWarehouseQueryExecutor DatabricksSqlWarehouseQueryExecutor { get; }

    public async Task<CalculationJobId> StartCalculationJobAsync(Calculation calculationJobInput)
    {
        var calculatorJobId = await DatabricksClient.GetCalculatorJobIdAsync();
        var runParameters = new DatabricksCalculationParametersFactory()
            .CreateParameters(calculationJobInput);

        var runId = await DatabricksClient
            .Jobs
            .RunNow(calculatorJobId, runParameters);

        DiagnosticMessageSink.WriteDiagnosticMessage($"'CalculatorJob' for {calculationJobInput.CalculationType} with id '{runId}' started.");

        return new CalculationJobId(runId);
    }

    public async Task<(bool IsCompleted, Run? Run)> WaitForCalculationJobCompletedAsync(
        CalculationJobId calculationJobId,
        TimeSpan waitTimeLimit)
    {
        var delay = TimeSpan.FromMinutes(2);

        (Run, RepairHistory) runState = default;
        CalculationState? calculationState = CalculationState.Pending;
        var isCondition = await Awaiter.TryWaitUntilConditionAsync(
            async () =>
            {
                runState = await DatabricksClient.Jobs.RunsGet(calculationJobId.Id);
                calculationState = ConvertToCalculationState(runState.Item1);

                return
                    calculationState is CalculationState.Completed
                    or CalculationState.Failed
                    or CalculationState.Canceled;
            },
            waitTimeLimit,
            delay);

        DiagnosticMessageSink.WriteDiagnosticMessage($"Wait for 'CalculatorJob' with id '{calculationJobId.Id}' completed with '{nameof(isCondition)}={isCondition}' and '{nameof(calculationState)}={calculationState}'.");

        return (calculationState == CalculationState.Completed, runState.Item1);
    }

    public async Task<Response<LogsQueryResult>> QueryLogAnalyticsAsync(string query, QueryTimeRange queryTimeRange)
    {
        return await LogsQueryClient.QueryWorkspaceAsync(Configuration.LogAnalyticsWorkspaceId, query, queryTimeRange);
    }

    public async Task<IReadOnlyList<(bool IsAccessible, string ErrorMessage)>> ArePublicDataModelsAccessibleAsync(
        IReadOnlyList<(string ModelName, string TableName)> modelsAndTables)
    {
        var results = new ConcurrentBag<(bool IsAccessible, string ErrorMessage)>();
        var tasks = modelsAndTables.Select(async item =>
        {
            try
            {
                var statement = DatabricksStatement.FromRawSql($"SELECT * FROM {Configuration.DatabricksCatalogName}.{item.ModelName}.{item.TableName} LIMIT 1");
                var queryResult = DatabricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(statement.Build());
                var list = await queryResult.ToListAsync();
                if (list.Count == 0)
                {
                    results.Add(new(
                        false,
                        $"Table '{item.TableName}' in model '{item.ModelName}' doesn't contain data."));
                }
                else
                {
                    results.Add(new(true, string.Empty));
                }
            }
            catch (Exception e)
            {
                results.Add(new(
                    false,
                    $"Table '{item.TableName}' in model '{item.ModelName}' is missing. Exception: {e.Message}"));
            }
        }).ToList();

        await Task.WhenAll(tasks);
        return results.ToList();
    }

    public async Task<(long? CalculationVersion, string Message)> GetLatestCalculationVersionFromCalculationsAsync()
    {
        try
        {
            var statement = DatabricksStatement.FromRawSql(
                $"SELECT calculation_version FROM {Configuration.DatabricksCatalogName}.wholesale_internal.calculations ORDER BY calculation_version DESC LIMIT 1");
            var queryResult = DatabricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(statement.Build());
            var item = await queryResult.FirstAsync();

            if (item.calculation_version != null)
            {
                return (item.calculation_version, "Calculation version retrieved successfully");
            }

            return (null, "No data found in the table");
        }
        catch (Exception e)
        {
            return (null, $"An error occurred: {e.Message}");
        }
    }

    public async Task<(long? CalculationVersion, string Message)> GetCalculationVersionOfCalculationIdFromCalculationsAsync(
        Guid calculationId)
    {
        try
        {
            var statement = DatabricksStatement.FromRawSql(
                $"SELECT calculation_version FROM {Configuration.DatabricksCatalogName}.wholesale_internal.calculations WHERE calculation_id = '{calculationId}'");
            var queryResult = DatabricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(statement.Build());
            var item = await queryResult.FirstAsync();

            if (item.calculation_version != null)
            {
                return (item.calculation_version, "Calculation ID retrieved successfully");
            }

            return (null, "No data found in the table");
        }
        catch (Exception e)
        {
            return (null, $"An error occurred: {e.Message}");
        }
    }

    protected override Task OnInitializeAsync()
    {
        DatabricksClient = DatabricksClient.CreateClient(Configuration.DatabricksWorkspace.BaseUrl, Configuration.DatabricksWorkspace.Token);

        return Task.CompletedTask;
    }

    protected override Task OnDisposeAsync()
    {
        DatabricksClient.Dispose();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Conversion rules was copied from "CalculationEngineClient".
    /// </summary>
    private static CalculationState ConvertToCalculationState(Run run)
    {
// TODO: Fix usage of obsolete RunLifeCycleState and RunResultState
#pragma warning disable CS0618 // Type or member is obsolete
        return run.State.LifeCycleState switch
        {
            RunLifeCycleState.PENDING => CalculationState.Pending,
            RunLifeCycleState.RUNNING => CalculationState.Running,
            RunLifeCycleState.TERMINATING => CalculationState.Running,
            RunLifeCycleState.SKIPPED => CalculationState.Canceled,
            RunLifeCycleState.INTERNAL_ERROR => CalculationState.Failed,
            RunLifeCycleState.TERMINATED => run.State.ResultState switch
            {
                RunResultState.SUCCESS => CalculationState.Completed,
                RunResultState.FAILED => CalculationState.Failed,
                RunResultState.CANCELED => CalculationState.Canceled,
                RunResultState.TIMEDOUT => CalculationState.Canceled,
                _ => throw new ArgumentOutOfRangeException(nameof(run.State)),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(run.State)),
        };
#pragma warning restore CS0618 // Type or member is obsolete
    }

    private DatabricksSqlWarehouseQueryExecutor GetDatabricksSqlWarehouseQueryExecutor()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            { "WorkspaceUrl", Configuration.DatabricksWorkspace.BaseUrl },
            { "WorkspaceToken", Configuration.DatabricksWorkspace.Token },
            { "WarehouseId", Configuration.DatabricksWorkspace.WarehouseId },
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDatabricksSqlStatementExecution(configuration);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        return serviceProvider.GetRequiredService<DatabricksSqlWarehouseQueryExecutor>();
    }
}

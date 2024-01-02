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

using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Wholesale.Batches.Application.Model;
using Energinet.DataHub.Wholesale.Batches.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Calculations;
using Energinet.DataHub.Wholesale.SubsystemTests.Features.Databricks.States;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Extensions;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.LazyFixture;
using Microsoft.Azure.Databricks.Client;
using Microsoft.Azure.Databricks.Client.Models;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.Databricks.Fixtures
{
    public sealed class CalculationJobScenarioFixture : LazyFixtureBase
    {
        public CalculationJobScenarioFixture(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
            Configuration = new CalculationJobScenarioConfiguration();
            ScenarioState = new CalculationJobScenarioState();
        }

        public CalculationJobScenarioState ScenarioState { get; }

        private CalculationJobScenarioConfiguration Configuration { get; }

        /// <summary>
        /// The actual client is not created until <see cref="OnInitializeAsync"/> has been called by the base class.
        /// </summary>
        private DatabricksClient DatabricksClient { get; set; } = null!;

        public async Task<CalculationId> StartCalculationJobAsync(Calculation calculationJobInput)
        {
            var calculatorJobId = await DatabricksClient.GetCalculatorJobIdAsync();
            var runParameters = new DatabricksCalculationParametersFactory()
                .CreateParameters(calculationJobInput);

            // TODO - Remove when fixed in migrations: temporary run on metering_point_periods_deduplicated_version_three
            runParameters.PythonParams.Add("--metering_point_periods_table_name=metering_point_periods_deduplicated_version_three");

            var runId = await DatabricksClient
                .Jobs
                .RunNow(calculatorJobId, runParameters);

            DiagnosticMessageSink.WriteDiagnosticMessage($"'CalculatorJob' for {calculationJobInput.ProcessType} with id '{runId}' started.");

            return new CalculationId(runId);
        }

        public async Task<(bool IsCompleted, Run? Run)> WaitForCalculationJobCompletedAsync(
            CalculationId calculationId,
            TimeSpan waitTimeLimit)
        {
            var delay = TimeSpan.FromMinutes(2);

            (Run, RepairHistory) runState = default;
            CalculationState? calculationState = CalculationState.Pending;
            var isCondition = await Awaiter.TryWaitUntilConditionAsync(
                async () =>
                {
                    runState = await DatabricksClient.Jobs.RunsGet(calculationId.Id);
                    calculationState = ConvertToCalculationState(runState.Item1);

                    return
                        calculationState == CalculationState.Completed
                        || calculationState == CalculationState.Failed
                        || calculationState == CalculationState.Canceled;
                },
                waitTimeLimit,
                delay);

            DiagnosticMessageSink.WriteDiagnosticMessage($"Wait for 'CalculatorJob' with id '{calculationId.Id}' completed with '{nameof(isCondition)}={isCondition}' and '{nameof(calculationState)}={calculationState}'.");

            return (calculationState == CalculationState.Completed, runState.Item1);
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
        }
    }
}

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

using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Energinet.DataHub.Wholesale.Calculations.Application;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.CalculationState;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Microsoft.Azure.Functions.Worker;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
internal class CalculationActivities
{
    private readonly IClock _clock;
    private readonly ICreateCalculationHandler _createCalculationHandler;
    private readonly IUnitOfWork _calculationUnitOfWork;
    private readonly ICalculationRepository _calculationRepository;
    private readonly ICalculationEngineClient _calculationEngineClient;
    private readonly ICalculationDtoMapper _calculationDtoMapper;
    private readonly ICompletedCalculationFactory _completedCalculationFactory;
    private readonly Energinet.DataHub.Wholesale.Events.Application.UseCases.IUnitOfWork _eventsUnitOfWork;
    private readonly ICompletedCalculationRepository _completedCalculationRepository;
    private readonly IPublisher _integrationEventsPublisher;

    public CalculationActivities(
        IClock clock,
        ICreateCalculationHandler createCalculationHandler,
        IUnitOfWork calculationUnitOfWork,
        ICalculationRepository calculationRepository,
        ICalculationEngineClient calculationEngineClient,
        ICalculationDtoMapper calculationDtoMapper,
        ICompletedCalculationFactory completedCalculationFactory,
        Energinet.DataHub.Wholesale.Events.Application.UseCases.IUnitOfWork eventsUnitOfWork,
        ICompletedCalculationRepository completedCalculationRepository,
        IPublisher integrationEventsPublisher)
    {
        _clock = clock;
        _createCalculationHandler = createCalculationHandler;
        _calculationUnitOfWork = calculationUnitOfWork;
        _calculationRepository = calculationRepository;
        _calculationEngineClient = calculationEngineClient;
        _calculationDtoMapper = calculationDtoMapper;
        _completedCalculationFactory = completedCalculationFactory;
        _eventsUnitOfWork = eventsUnitOfWork;
        _completedCalculationRepository = completedCalculationRepository;
        _integrationEventsPublisher = integrationEventsPublisher;
    }

    /// <summary>
    /// Create calculation status record in SQL database.
    /// </summary>
    [Function(nameof(CreateCalculationRecordActivity))]
    public async Task<CalculationMetadata> CreateCalculationRecordActivity(
        [ActivityTrigger] CalculationRequestDto calculationRequestDto)
    {
        // TODO: Temporary solution for user id
        var userId = Guid.Parse("3A3A90B7-C624-4844-B990-3221DEE54F04");
        var calculationId = await _createCalculationHandler.HandleAsync(new CreateCalculationCommand(
            calculationRequestDto.CalculationType,
            calculationRequestDto.GridAreaCodes,
            calculationRequestDto.StartDate,
            calculationRequestDto.EndDate,
            userId));

        return new CalculationMetadata
        {
            Id = calculationId,
            Input = calculationRequestDto,
        };
    }

    /// <summary>
    /// Update calculation status record in SQL database.
    /// </summary>
    [Function(nameof(UpdateCalculationExecutionStatusActivity))]
    public async Task UpdateCalculationExecutionStatusActivity(
        [ActivityTrigger] CalculationMetadata calculationMetadata)
    {
        var calculation = await _calculationRepository.GetAsync(calculationMetadata.Id);
        var newExecutionState = CalculationStateMapper.MapState(calculationMetadata.JobStatus);

        if (calculation.ExecutionState != newExecutionState)
        {
            switch (newExecutionState)
            {
                case CalculationExecutionState.Pending:
                    calculation.MarkAsPending();
                    break;
                case CalculationExecutionState.Executing:
                    calculation.MarkAsExecuting();
                    break;
                case CalculationExecutionState.Completed:
                    calculation.MarkAsCompleted(_clock.GetCurrentInstant());
                    break;
                case CalculationExecutionState.Failed:
                    calculation.MarkAsFailed();
                    break;
                case CalculationExecutionState.Canceled:
                    // Jobs may be cancelled in Databricks for various reasons. For example they can be cancelled due to migrations in CD
                    // Setting calculation state back to "created" ensure they will be picked up and started again
                    calculation.Reset();
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unexpected execution state: {newExecutionState.ToString()}.");
            }

            await _calculationUnitOfWork.CommitAsync();
        }
    }

    /// <summary>
    /// Update calculation status record in SQL database.
    /// </summary>
    [Function(nameof(CreateCompletedCalculationActivity))]
    public async Task CreateCompletedCalculationActivity(
        [ActivityTrigger] Guid calculationdId)
    {
        var calculation = await _calculationRepository.GetAsync(calculationdId);
        var calculationDto = _calculationDtoMapper.Map(calculation);

        var completedCalculations = _completedCalculationFactory.CreateFromCalculations([calculationDto]);
        await _completedCalculationRepository.AddAsync(completedCalculations);
        await _eventsUnitOfWork.CommitAsync();
    }

    /// <summary>
    /// Start calculation in Databricks.
    /// </summary>
    [Function(nameof(StartCalculationActivity))]
    public async Task<CalculationJobId> StartCalculationActivity(
        [ActivityTrigger] Guid calculationdId)
    {
        var calculation = await _calculationRepository.GetAsync(calculationdId);
        var jobId = await _calculationEngineClient.StartAsync(calculation);
        calculation.MarkAsSubmitted(jobId);
        await _calculationUnitOfWork.CommitAsync();

        return jobId;
    }

    /// <summary>
    /// Request calculation job status in Databricks.
    /// </summary>
    [Function(nameof(GetJobStatusActivity))]
    public async Task<Energinet.DataHub.Wholesale.Calculations.Application.Model.CalculationState> GetJobStatusActivity(
        [ActivityTrigger] CalculationJobId jobId)
    {
        var calculationState = await _calculationEngineClient.GetStatusAsync(jobId);

        return calculationState;
    }

    /// <summary>
    /// Retrieve calculation results from Databricks and send them as events using ServiceBus.
    /// </summary>
    [Function(nameof(SendCalculationResultsActivity))]
    public async Task SendCalculationResultsActivity(
        [ActivityTrigger] Guid calculationId)
    {
        await _integrationEventsPublisher.PublishAsync(CancellationToken.None);
    }
}
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

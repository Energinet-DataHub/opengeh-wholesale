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

using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.Wholesale.Calculations.Application;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Security;
using Energinet.DataHub.Wholesale.Orchestrations.Extensions.Options;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.ScheduleCalculation;

public class CalculationSchedulerHandler(
    ILogger<CalculationSchedulerHandler> logger,
    IOptions<CalculationOrchestrationMonitorOptions> orchestrationMonitorOptions,
    IClock clock,
    IUserContext<FrontendUser> userContext,
    ICalculationRepository calculationRepository,
    IUnitOfWork unitOfWork)
{
    private readonly ILogger _logger = logger;
    private readonly CalculationOrchestrationMonitorOptions _orchestrationMonitorOptions = orchestrationMonitorOptions.Value;
    private readonly IClock _clock = clock;
    private readonly IUserContext<FrontendUser> _userContext = userContext;
    private readonly ICalculationRepository _calculationRepository = calculationRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task StartScheduledCalculationsAsync(DurableTaskClient durableTaskClient)
    {
        var now = _clock.GetCurrentInstant();
        var scheduledCalculationIds = await _calculationRepository
            .GetScheduledCalculationsAsync(scheduledToRunBefore: now)
            .ConfigureAwait(false);

        var calculationStarter = new CalculationStarter(
            _logger,
            _orchestrationMonitorOptions,
            durableTaskClient);

        foreach (var calculationToStart in scheduledCalculationIds)
        {
            try
            {
                await calculationStarter
                    .StartCalculationAsync(calculationToStart)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                // Log error if orchestration did not start successfully.
                // Does not throw exception since we want to continue processing the next scheduled calculations.
                _logger.LogError(
                    e,
                    "Failed to start calculation with id = {CalculationId} and orchestration instance id = {OrchestrationInstanceId}",
                    calculationToStart.CalculationId.Id,
                    calculationToStart.OrchestrationInstanceId.Id);
            }
        }
    }

    public async Task CancelScheduledCalculationAsync(DurableTaskClient durableTaskClient, CalculationId calculationId)
    {
        var scheduledCalculation = await _calculationRepository.GetAsync(calculationId.Id)
            .ConfigureAwait(false);

        if (!scheduledCalculation.CanCancel())
        {
            throw new InvalidOperationException($"Unable to cancel calculation with id = {scheduledCalculation.Id} " +
                                                $"and status = {scheduledCalculation.OrchestrationState}");
        }

        var existingOrchestration = await durableTaskClient
            .GetInstanceAsync(scheduledCalculation.OrchestrationInstanceId.Id)
            .ConfigureAwait(false);

        if (existingOrchestration != null)
        {
            throw new InvalidOperationException($"Unable to cancel calculation with id = {scheduledCalculation.Id} " +
                                                $"and orchestration id = {scheduledCalculation.OrchestrationInstanceId.Id} " +
                                                $"since the orchestration is already started");
        }

        scheduledCalculation.MarkAsCanceled(_userContext.CurrentUser.UserId);

        await _unitOfWork.CommitAsync()
            .ConfigureAwait(false);

        _logger.LogInformation(
            "Calculation with id {calculationId} was cancelled",
            calculationId.Id);
    }
}

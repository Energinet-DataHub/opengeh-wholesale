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

using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;

namespace Energinet.DataHub.Wholesale.Infrastructure.Integration;

public interface ICalculationResultReadyIntegrationEventFactory
{
    CalculationResultCompleted CreateCalculationResultCompletedForGridArea(
        ProcessStepResult processStepResultDto,
        ProcessCompletedEventDto processCompletedEventDto);

    CalculationResultCompleted CreateCalculationResultCompletedForEnergySupplier(
        ProcessStepResult processStepResultDto,
        ProcessCompletedEventDto processCompletedEventDto,
        string energySupplierGln);

    CalculationResultCompleted CreateCalculationResultCompletedForBalanceResponsibleParty(
        ProcessStepResult processStepResultDto,
        ProcessCompletedEventDto processCompletedEventDto,
        string balanceResponsiblePartyGln);

    CalculationResultCompleted CreateCalculationResultForEnergySupplierByBalanceResponsibleParty(
        ProcessStepResult result,
        ProcessCompletedEventDto processCompletedEvent,
        string energySupplierGln,
        string balanceResponsiblePartyGln);
}

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

using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using Energinet.DataHub.Wholesale.Events.Application.Communication;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.CalculationCompletedV1.Factories;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EventProviders;

public class CalculationCompletedEventProvider : ICalculationCompletedEventProvider
{
    private readonly ICalculationCompletedFactory _calculationCompletedFactory;

    public CalculationCompletedEventProvider(ICalculationCompletedFactory calculationCompletedFactory)
    {
        _calculationCompletedFactory = calculationCompletedFactory;
    }

    public IntegrationEvent Get(CalculationDto unpublishedCalculation, string orchestrationInstanceId)
    {
        var calculationCompletedV1 = _calculationCompletedFactory.Create(
            unpublishedCalculation.CalculationId,
            orchestrationInstanceId,
            unpublishedCalculation.CalculationType,
            unpublishedCalculation.Version);

        return new IntegrationEvent(
            EventIdentification: unpublishedCalculation.CalculationId,
            Contracts.IntegrationEvents.CalculationCompletedV1.EventName,
            Contracts.IntegrationEvents.CalculationCompletedV1.EventMinorVersion,
            calculationCompletedV1);
    }
}

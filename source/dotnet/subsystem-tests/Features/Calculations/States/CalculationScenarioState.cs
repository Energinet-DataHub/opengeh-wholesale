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

using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.SubsystemTests.Clients.v3;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.Calculations.States;

public class CalculationScenarioState
{
    public CalculationRequestDto CalculationInput { get; set; }
        = new();

    public IList<string> SubscribedIntegrationEventNames { get; }
        = new List<string>();

    public Guid CalculationId { get; set; }

    public CalculationDto? Calculation { get; set; }

    public IReadOnlyCollection<EnergyResultProducedV2> ReceivedEnergyResultProducedV2 { get; set; }
        = new List<EnergyResultProducedV2>();

    public IReadOnlyCollection<GridLossResultProducedV1> ReceivedGridLossProducedV1 { get; set; }
        = new List<GridLossResultProducedV1>();

    public IReadOnlyCollection<AmountPerChargeResultProducedV1> ReceivedAmountPerChargeResultProducedV1 { get; set; }
        = new List<AmountPerChargeResultProducedV1>();

    public IReadOnlyCollection<MonthlyAmountPerChargeResultProducedV1> ReceivedMonthlyAmountPerChargeResultProducedV1 { get; set; }
        = new List<MonthlyAmountPerChargeResultProducedV1>();
}

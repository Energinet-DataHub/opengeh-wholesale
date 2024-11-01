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

using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Energinet.DataHub.Wholesale.SubsystemTests.Clients.v3;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.Calculations.States;

public class CalculationScenarioState
{
    public StartCalculationRequestDto? CalculationInput { get; set; }

    public IList<string> SubscribedIntegrationEventNames { get; }
        = [];

    public Guid CalculationId { get; set; }

    public string OrchestrationInstanceId { get; set; } = string.Empty;

    public CalculationDto? Calculation { get; set; }

    public IReadOnlyCollection<CalculationCompletedV1> ReceivedCalculationCompletedV1 { get; set; } = [];
}

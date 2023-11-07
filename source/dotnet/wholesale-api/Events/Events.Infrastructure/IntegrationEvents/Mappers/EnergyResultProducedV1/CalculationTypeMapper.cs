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

using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using CalculationType = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.EnergyResultProducedV1.Types.CalculationType;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers.EnergyResultProducedV1;

public static class CalculationTypeMapper
{
    public static CalculationType MapCalculationType(ProcessType processType)
    {
        return processType switch
        {
            ProcessType.Aggregation => CalculationType.Aggregation,
            ProcessType.BalanceFixing => CalculationType.BalanceFixing,
            ProcessType.WholesaleFixing => CalculationType.WholesaleFixing,
            ProcessType.FirstCorrectionSettlement => CalculationType.FirstCorrectionSettlement,
            ProcessType.SecondCorrectionSettlement => CalculationType.SecondCorrectionSettlement,
            ProcessType.ThirdCorrectionSettlement => CalculationType.ThirdCorrectionSettlement,

            _ => throw new ArgumentOutOfRangeException(
                nameof(processType),
                actualValue: processType,
                "Value cannot be mapped to a calculation type."),
        };
    }
}

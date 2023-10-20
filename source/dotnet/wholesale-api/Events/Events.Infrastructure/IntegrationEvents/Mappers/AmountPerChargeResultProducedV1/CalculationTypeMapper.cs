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

using EventCalculationType = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.AmountPerChargeResultProducedV1.Types.CalculationType;
using ModelCalculationType = Energinet.DataHub.Wholesale.Common.Models.ProcessType;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers.AmountPerChargeResultProducedV1;

public static class CalculationTypeMapper
{
    public static EventCalculationType MapCalculationType(ModelCalculationType calculationType)
    {
        return calculationType switch
        {
            ModelCalculationType.WholesaleFixing => EventCalculationType.WholesaleFixing,
            ModelCalculationType.FirstCorrectionSettlement => EventCalculationType.FirstCorrectionSettlement,
            ModelCalculationType.SecondCorrectionSettlement => EventCalculationType.SecondCorrectionSettlement,
            ModelCalculationType.ThirdCorrectionSettlement => EventCalculationType.ThirdCorrectionSettlement,
            _ => throw new ArgumentOutOfRangeException(nameof(calculationType), actualValue: calculationType, "Unexpected calculationType."),
        };
    }
}

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

using CalculationType = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.AmountPerChargeResultProducedV1.Types.CalculationType;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers.AmountPerChargeResultProducedV1;

public static class CalculationTypeMapper
{
    public static CalculationType MapCalculationType(Common.Models.ProcessType calculationType)
    {
        return calculationType switch
        {
            Common.Models.ProcessType.WholesaleFixing => CalculationType.WholesaleFixing,
            Common.Models.ProcessType.FirstCorrectionSettlement => CalculationType.FirstCorrectionSettlement,
            Common.Models.ProcessType.SecondCorrectionSettlement => CalculationType.SecondCorrectionSettlement,
            Common.Models.ProcessType.ThirdCorrectionSettlement => CalculationType.ThirdCorrectionSettlement,
            _ => throw new ArgumentOutOfRangeException(nameof(calculationType), actualValue: calculationType, "Unexpected calculationType."),
        };
    }
}

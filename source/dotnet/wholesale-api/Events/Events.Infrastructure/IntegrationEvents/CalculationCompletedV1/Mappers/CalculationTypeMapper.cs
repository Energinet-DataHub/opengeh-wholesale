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

using CalculationType = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.CalculationCompletedV1.Types.CalculationType;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.CalculationCompletedV1.Mappers;

public static class CalculationTypeMapper
{
    public static CalculationType MapCalculationType(Wholesale.Common.Interfaces.Models.CalculationType calculationType)
    {
        return calculationType switch
        {
            Wholesale.Common.Interfaces.Models.CalculationType.Aggregation => CalculationType.Aggregation,
            Wholesale.Common.Interfaces.Models.CalculationType.BalanceFixing => CalculationType.BalanceFixing,
            Wholesale.Common.Interfaces.Models.CalculationType.WholesaleFixing => CalculationType.WholesaleFixing,
            Wholesale.Common.Interfaces.Models.CalculationType.FirstCorrectionSettlement => CalculationType.FirstCorrectionSettlement,
            Wholesale.Common.Interfaces.Models.CalculationType.SecondCorrectionSettlement => CalculationType.SecondCorrectionSettlement,
            Wholesale.Common.Interfaces.Models.CalculationType.ThirdCorrectionSettlement => CalculationType.ThirdCorrectionSettlement,

            _ => throw new ArgumentOutOfRangeException(
                nameof(calculationType),
                actualValue: calculationType,
                "Value cannot be mapped to a calculation type."),
        };
    }
}

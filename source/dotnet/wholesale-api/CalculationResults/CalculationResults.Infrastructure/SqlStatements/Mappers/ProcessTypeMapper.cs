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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;

public static class CalculationTypeMapper
{
    public static CalculationType FromDeltaTableValue(string calculationType)
    {
        return calculationType switch
        {
            DeltaTableConstants.DeltaTableCalculationType.BalanceFixing => CalculationType.BalanceFixing,
            DeltaTableConstants.DeltaTableCalculationType.Aggregation => CalculationType.Aggregation,
            DeltaTableConstants.DeltaTableCalculationType.WholesaleFixing => CalculationType.WholesaleFixing,
            DeltaTableConstants.DeltaTableCalculationType.FirstCorrectionSettlement => CalculationType.FirstCorrectionSettlement,
            DeltaTableConstants.DeltaTableCalculationType.SecondCorrectionSettlement => CalculationType.SecondCorrectionSettlement,
            DeltaTableConstants.DeltaTableCalculationType.ThirdCorrectionSettlement => CalculationType.ThirdCorrectionSettlement,

            _ => throw new ArgumentOutOfRangeException(
                nameof(calculationType),
                actualValue: calculationType,
                "Value does not contain a valid string representation of a calculation type."),
        };
    }

    public static string ToDeltaTableValue(CalculationType calculationType)
    {
        return calculationType switch
        {
            CalculationType.BalanceFixing => DeltaTableConstants.DeltaTableCalculationType.BalanceFixing,
            CalculationType.Aggregation => DeltaTableConstants.DeltaTableCalculationType.Aggregation,
            CalculationType.WholesaleFixing => DeltaTableConstants.DeltaTableCalculationType.WholesaleFixing,
            CalculationType.FirstCorrectionSettlement => DeltaTableConstants.DeltaTableCalculationType.FirstCorrectionSettlement,
            CalculationType.SecondCorrectionSettlement => DeltaTableConstants.DeltaTableCalculationType.SecondCorrectionSettlement,
            CalculationType.ThirdCorrectionSettlement => DeltaTableConstants.DeltaTableCalculationType.ThirdCorrectionSettlement,

            _ => throw new ArgumentOutOfRangeException(
                nameof(calculationType),
                actualValue: calculationType,
                "Value cannot be mapped to a string representation of a calculation type."),
        };
    }
}

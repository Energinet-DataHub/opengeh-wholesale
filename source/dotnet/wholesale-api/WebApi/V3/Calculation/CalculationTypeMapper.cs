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

namespace Energinet.DataHub.Wholesale.WebApi.V3.Calculation;

public static class CalculationTypeMapper
{
    public static Common.Interfaces.Models.CalculationType Map(CalculationType batchDtoProcessType)
    {
        return batchDtoProcessType switch
        {
            CalculationType.Aggregation => Common.Interfaces.Models.CalculationType.Aggregation,
            CalculationType.BalanceFixing => Common.Interfaces.Models.CalculationType.BalanceFixing,
            CalculationType.WholesaleFixing => Common.Interfaces.Models.CalculationType.WholesaleFixing,
            CalculationType.FirstCorrectionSettlement => Common.Interfaces.Models.CalculationType.FirstCorrectionSettlement,
            CalculationType.SecondCorrectionSettlement => Common.Interfaces.Models.CalculationType.SecondCorrectionSettlement,
            CalculationType.ThirdCorrectionSettlement => Common.Interfaces.Models.CalculationType.ThirdCorrectionSettlement,

            _ => throw new ArgumentOutOfRangeException(
                nameof(batchDtoProcessType),
                actualValue: batchDtoProcessType,
                "Value cannot be mapped to a calculation type."),
        };
    }

    public static CalculationType Map(Common.Interfaces.Models.CalculationType batchDtoProcessType)
    {
        return batchDtoProcessType switch
        {
            Common.Interfaces.Models.CalculationType.Aggregation => CalculationType.Aggregation,
            Common.Interfaces.Models.CalculationType.BalanceFixing => CalculationType.BalanceFixing,
            Common.Interfaces.Models.CalculationType.WholesaleFixing => CalculationType.WholesaleFixing,
            Common.Interfaces.Models.CalculationType.FirstCorrectionSettlement => CalculationType.FirstCorrectionSettlement,
            Common.Interfaces.Models.CalculationType.SecondCorrectionSettlement => CalculationType.SecondCorrectionSettlement,
            Common.Interfaces.Models.CalculationType.ThirdCorrectionSettlement => CalculationType.ThirdCorrectionSettlement,

            _ => throw new ArgumentOutOfRangeException(
                nameof(batchDtoProcessType),
                actualValue: batchDtoProcessType,
                "Value cannot be mapped to a calculation type."),
        };
    }
}

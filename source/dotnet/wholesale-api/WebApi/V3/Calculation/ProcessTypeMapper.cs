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

public static class ProcessTypeMapper
{
    public static Common.Interfaces.Models.ProcessType Map(ProcessType batchDtoProcessType)
    {
        return batchDtoProcessType switch
        {
            ProcessType.Aggregation => Common.Interfaces.Models.ProcessType.Aggregation,
            ProcessType.BalanceFixing => Common.Interfaces.Models.ProcessType.BalanceFixing,
            ProcessType.WholesaleFixing => Common.Interfaces.Models.ProcessType.WholesaleFixing,
            ProcessType.FirstCorrectionSettlement => Common.Interfaces.Models.ProcessType.FirstCorrectionSettlement,
            ProcessType.SecondCorrectionSettlement => Common.Interfaces.Models.ProcessType.SecondCorrectionSettlement,
            ProcessType.ThirdCorrectionSettlement => Common.Interfaces.Models.ProcessType.ThirdCorrectionSettlement,

            _ => throw new ArgumentOutOfRangeException(
                nameof(batchDtoProcessType),
                actualValue: batchDtoProcessType,
                "Value cannot be mapped to a process type."),
        };
    }

    public static ProcessType Map(Common.Interfaces.Models.ProcessType batchDtoProcessType)
    {
        return batchDtoProcessType switch
        {
            Common.Interfaces.Models.ProcessType.Aggregation => ProcessType.Aggregation,
            Common.Interfaces.Models.ProcessType.BalanceFixing => ProcessType.BalanceFixing,
            Common.Interfaces.Models.ProcessType.WholesaleFixing => ProcessType.WholesaleFixing,
            Common.Interfaces.Models.ProcessType.FirstCorrectionSettlement => ProcessType.FirstCorrectionSettlement,
            Common.Interfaces.Models.ProcessType.SecondCorrectionSettlement => ProcessType.SecondCorrectionSettlement,
            Common.Interfaces.Models.ProcessType.ThirdCorrectionSettlement => ProcessType.ThirdCorrectionSettlement,

            _ => throw new ArgumentOutOfRangeException(
                nameof(batchDtoProcessType),
                actualValue: batchDtoProcessType,
                "Value cannot be mapped to a process type."),
        };
    }
}

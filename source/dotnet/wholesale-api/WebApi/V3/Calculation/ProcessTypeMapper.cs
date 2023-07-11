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
    public static Common.Models.ProcessType Map(ProcessType calculationDtoProcessType)
    {
        return calculationDtoProcessType switch
        {
            ProcessType.Aggregation => Common.Models.ProcessType.Aggregation,
            ProcessType.BalanceFixing => Common.Models.ProcessType.BalanceFixing,
            _ => throw new ArgumentOutOfRangeException(nameof(calculationDtoProcessType), calculationDtoProcessType, null),
        };
    }

    public static ProcessType Map(Common.Models.ProcessType calculationDtoProcessType)
    {
        return calculationDtoProcessType switch
        {
            Common.Models.ProcessType.Aggregation => ProcessType.Aggregation,
            Common.Models.ProcessType.BalanceFixing => ProcessType.BalanceFixing,
            _ => throw new ArgumentOutOfRangeException(nameof(calculationDtoProcessType), calculationDtoProcessType, null),
        };
    }
}

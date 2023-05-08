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

namespace Energinet.DataHub.Wholesale.WebApi.V3.Batch;

public static class ProcessTypeMapper
{
    public static Batches.Interfaces.Models.ProcessType MapProcessType(ProcessType batchDtoProcessType)
    {
        return batchDtoProcessType switch
        {
            ProcessType.Aggregation => Batches.Interfaces.Models.ProcessType.Aggregation,
            ProcessType.BalanceFixing => Batches.Interfaces.Models.ProcessType.BalanceFixing,
            _ => throw new ArgumentOutOfRangeException(nameof(batchDtoProcessType), batchDtoProcessType, null),
        };
    }

    public static ProcessType MapProcessType(Batches.Interfaces.Models.ProcessType batchDtoProcessType)
    {
        return batchDtoProcessType switch
        {
            Batches.Interfaces.Models.ProcessType.Aggregation => ProcessType.Aggregation,
            Batches.Interfaces.Models.ProcessType.BalanceFixing => ProcessType.BalanceFixing,
            _ => throw new ArgumentOutOfRangeException(nameof(batchDtoProcessType), batchDtoProcessType, null),
        };
    }
}

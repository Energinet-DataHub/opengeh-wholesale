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

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers.Common;

public static class ProcessTypeMapper
{
    public static ProcessType MapProcessType(Wholesale.Common.Models.ProcessType processType)
    {
        return processType switch
        {
            Wholesale.Common.Models.ProcessType.Aggregation => ProcessType.Aggregation,
            Wholesale.Common.Models.ProcessType.BalanceFixing => ProcessType.BalanceFixing,
            Wholesale.Common.Models.ProcessType.WholesaleFixing => ProcessType.WholesaleFixing,
            Wholesale.Common.Models.ProcessType.FirstCorrectionSettlement => ProcessType.FirstCorrectionSettlement,
            Wholesale.Common.Models.ProcessType.SecondCorrectionSettlement => ProcessType.SecondCorrectionSettlement,
            Wholesale.Common.Models.ProcessType.ThirdCorrectionSettlement => ProcessType.ThirdCorrectionSettlement,
            _ => throw new ArgumentException($"No matching 'ProcessType' for: {processType.ToString()}"),
        };
    }
}

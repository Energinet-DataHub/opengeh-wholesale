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
using Energinet.DataHub.Wholesale.EDI.Models;

namespace Energinet.DataHub.Wholesale.EDI.Mappers;

public static class ProcessTypeMapper
{
    /// <summary>
    /// Maps a <see cref="RequestedProcessType"/> to a <see cref="ProcessType"/>. Cannot map <see cref="RequestedProcessType.LatestCorrection"/> to a <see cref="ProcessType"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Throws a ArgumentOutOfRangeException if the request process type is unknown or has the value LatestCorrection</exception>
    public static ProcessType FromRequestedProcessType(RequestedProcessType requestedProcessType)
    {
        if (requestedProcessType == RequestedProcessType.LatestCorrection)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedProcessType),
                actualValue: requestedProcessType,
                "Requested process type (RequestedProcessType.LatestCorrection) cannot be mapped to process type.");
        }

        return requestedProcessType switch
        {
            RequestedProcessType.BalanceFixing => ProcessType.BalanceFixing,
            RequestedProcessType.PreliminaryAggregation => ProcessType.Aggregation,
            RequestedProcessType.WholesaleFixing => ProcessType.WholesaleFixing,
            RequestedProcessType.FirstCorrection => ProcessType.FirstCorrectionSettlement,
            RequestedProcessType.SecondCorrection => ProcessType.SecondCorrectionSettlement,
            RequestedProcessType.ThirdCorrection => ProcessType.ThirdCorrectionSettlement,
            _ => throw new ArgumentOutOfRangeException(nameof(requestedProcessType), actualValue: requestedProcessType, "Requested process type cannot be mapped to process type."),
        };
    }
}

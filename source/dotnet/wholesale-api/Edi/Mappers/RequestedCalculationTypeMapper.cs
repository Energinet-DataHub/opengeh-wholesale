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

using Energinet.DataHub.Wholesale.EDI.Models;

namespace Energinet.DataHub.Wholesale.EDI.Mappers;

public static class RequestedCalculationTypeMapper
{
    public static RequestedCalculationType ToRequestedCalculationType(string businessReason, string? settlementSeriesVersion)
    {
        return businessReason != BusinessReason.Correction && settlementSeriesVersion != null
            ? throw new ArgumentOutOfRangeException(
                nameof(settlementSeriesVersion),
                settlementSeriesVersion,
                $"Value must be null when {nameof(BusinessReason)} is not {nameof(BusinessReason.Correction)}.")
            : businessReason switch
            {
                BusinessReason.BalanceFixing => RequestedCalculationType.BalanceFixing,
                BusinessReason.PreliminaryAggregation => RequestedCalculationType.PreliminaryAggregation,
                BusinessReason.WholesaleFixing => RequestedCalculationType.WholesaleFixing,
                BusinessReason.Correction => settlementSeriesVersion switch
                {
                    SettlementSeriesVersion.FirstCorrection => RequestedCalculationType.FirstCorrection,
                    SettlementSeriesVersion.SecondCorrection => RequestedCalculationType.SecondCorrection,
                    SettlementSeriesVersion.ThirdCorrection => RequestedCalculationType.ThirdCorrection,
                    null => RequestedCalculationType.LatestCorrection,
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(settlementSeriesVersion),
                        settlementSeriesVersion,
                        $"Value cannot be mapped to a {nameof(RequestedCalculationType)}."),
                },
                _ => throw new ArgumentOutOfRangeException(
                    nameof(businessReason),
                    businessReason,
                    $"Value cannot be mapped to a {nameof(RequestedCalculationType)}."),
            };
    }
}

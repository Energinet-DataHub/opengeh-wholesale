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

using Energinet.DataHub.Wholesale.Edi.Contracts;
using Energinet.DataHub.Wholesale.Edi.Models;

namespace Energinet.DataHub.Wholesale.Edi.Mappers;

public static class RequestedCalculationTypeMapper
{
    public static RequestedCalculationType ToRequestedCalculationType(string businessReason, string? settlementVersion)
    {
        if (businessReason != DataHubNames.BusinessReason.Correction && settlementVersion != null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(settlementVersion),
                settlementVersion,
                $"Value must be null when {nameof(businessReason)} is not {nameof(DataHubNames.BusinessReason.Correction)}.");
        }

        return businessReason switch
        {
            DataHubNames.BusinessReason.BalanceFixing => RequestedCalculationType.BalanceFixing,
            DataHubNames.BusinessReason.PreliminaryAggregation => RequestedCalculationType.PreliminaryAggregation,
            DataHubNames.BusinessReason.WholesaleFixing => RequestedCalculationType.WholesaleFixing,
            DataHubNames.BusinessReason.Correction => settlementVersion switch
            {
                DataHubNames.SettlementVersion.FirstCorrection => RequestedCalculationType.FirstCorrection,
                DataHubNames.SettlementVersion.SecondCorrection => RequestedCalculationType.SecondCorrection,
                DataHubNames.SettlementVersion.ThirdCorrection => RequestedCalculationType.ThirdCorrection,
                null => RequestedCalculationType.LatestCorrection,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(settlementVersion),
                    settlementVersion,
                    $"Value cannot be mapped to a {nameof(RequestedCalculationType)}."),
            },
            _ => throw new ArgumentOutOfRangeException(
                nameof(businessReason),
                businessReason,
                $"Value cannot be mapped to a {nameof(RequestedCalculationType)}."),
        };
    }
}

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

using Energinet.DataHub.Wholesale.Common.Models;
using Energinet.DataHub.Wholesale.EDI.Models;

namespace Energinet.DataHub.Wholesale.EDI.Mappers;

public static class ProcessTypeMapper
{
    public static ProcessType FromBusinessReason(string businessReason, string? settlementSeriesVersion)
    {
        if (businessReason != BusinessReason.Correction && !string.IsNullOrEmpty(settlementSeriesVersion))
            throw new ArgumentException("Settlement series version must be null when business reason isn't Correction", nameof(settlementSeriesVersion));

        return businessReason switch
        {
            BusinessReason.BalanceFixing => ProcessType.BalanceFixing,
            BusinessReason.PreliminaryAggregation => ProcessType.Aggregation,
            BusinessReason.WholesaleFixing => ProcessType.WholesaleFixing,
            BusinessReason.Correction => settlementSeriesVersion switch
            {
                SettlementSeriesVersion.FirstCorrection => ProcessType.FirstCorrectionSettlement,
                SettlementSeriesVersion.SecondCorrection => ProcessType.SecondCorrectionSettlement,
                SettlementSeriesVersion.ThirdCorrection => ProcessType.ThirdCorrectionSettlement,
                _ => throw new ArgumentOutOfRangeException(nameof(settlementSeriesVersion), settlementSeriesVersion, "Settlement series version cannot be mapped to a process type"),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(businessReason), actualValue: businessReason, "Business reason cannot be mapped to process type."),
        };
    }

    public static (string BusinessReason, string? SettlementSeriesVersion) ToBusinessReason(ProcessType processType)
    {
        return processType switch
        {
            ProcessType.Aggregation => (BusinessReason.PreliminaryAggregation, null),
            ProcessType.BalanceFixing => (BusinessReason.BalanceFixing, null),
            ProcessType.WholesaleFixing => (BusinessReason.WholesaleFixing, null),
            ProcessType.FirstCorrectionSettlement => (BusinessReason.Correction, SettlementSeriesVersion.FirstCorrection),
            ProcessType.SecondCorrectionSettlement => (BusinessReason.Correction, SettlementSeriesVersion.SecondCorrection),
            ProcessType.ThirdCorrectionSettlement => (BusinessReason.Correction, SettlementSeriesVersion.ThirdCorrection),

            _ => throw new ArgumentOutOfRangeException(nameof(processType), actualValue: processType, "Process type cannot be mapped to business reason."),
        };
    }
}

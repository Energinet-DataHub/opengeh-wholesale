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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;

public static class ProcessTypeMapper
{
    public static ProcessType FromDeltaTableValue(string processType) =>
        processType switch
        {
            DeltaTableConstants.DeltaTableProcessType.BalanceFixing => ProcessType.BalanceFixing,
            DeltaTableConstants.DeltaTableProcessType.Aggregation => ProcessType.Aggregation,
            DeltaTableConstants.DeltaTableProcessType.WholesaleFixing => ProcessType.WholesaleFixing,
            DeltaTableConstants.DeltaTableProcessType.FirstCorrectionSettlement => ProcessType.FirstCorrectionSettlement,
            DeltaTableConstants.DeltaTableProcessType.SecondCorrectionSettlement => ProcessType.SecondCorrectionSettlement,
            DeltaTableConstants.DeltaTableProcessType.ThirdCorrectionSettlement => ProcessType.ThirdCorrectionSettlement,
            _ => throw new FormatException($"Value does not contain a valid string representation of a process type. Value: '{processType}'."),
        };

    public static string ToDeltaTableValue(ProcessType processType) =>
        processType switch
        {
            ProcessType.BalanceFixing => DeltaTableConstants.DeltaTableProcessType.BalanceFixing,
            ProcessType.Aggregation => DeltaTableConstants.DeltaTableProcessType.Aggregation,
            ProcessType.WholesaleFixing => DeltaTableConstants.DeltaTableProcessType.WholesaleFixing,
            ProcessType.FirstCorrectionSettlement => DeltaTableConstants.DeltaTableProcessType.FirstCorrectionSettlement,
            ProcessType.SecondCorrectionSettlement => DeltaTableConstants.DeltaTableProcessType.SecondCorrectionSettlement,
            ProcessType.ThirdCorrectionSettlement => DeltaTableConstants.DeltaTableProcessType.ThirdCorrectionSettlement,
            _ => throw new ArgumentOutOfRangeException(nameof(processType), actualValue: processType, "Value cannot be mapped to a string representation of a process type."),
        };
}

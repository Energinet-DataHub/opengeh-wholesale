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

namespace Energinet.DataHub.Wholesale.Edi.Validation.Helpers;

public static class SettlementVersionValidationHelper
{
    private static readonly IReadOnlyList<string> _validSettlementVersions = new List<string>
    {
        DataHubNames.SettlementVersion.FirstCorrection,
        DataHubNames.SettlementVersion.SecondCorrection,
        DataHubNames.SettlementVersion.ThirdCorrection,
    };

    public static bool IsSettlementVersionValid(string businessReason, string? settlementVersion)
    {
        var isCorrection = businessReason == DataHubNames.BusinessReason.Correction;

        if (!isCorrection && settlementVersion != null)
            return false;

        if (!isCorrection)
            return true;

        // If the business reason is correction and settlement version is not set, latest correction result is requested
        if (settlementVersion == null)
            return true;

        if (!_validSettlementVersions.Contains(settlementVersion))
            return false;

        return true;
    }
}

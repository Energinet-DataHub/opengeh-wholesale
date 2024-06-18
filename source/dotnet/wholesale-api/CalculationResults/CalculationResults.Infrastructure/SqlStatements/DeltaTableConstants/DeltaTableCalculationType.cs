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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;

public static class DeltaTableCalculationType
{
    public const string Aggregation = "aggregation";
    public const string BalanceFixing = "balance_fixing";
    public const string WholesaleFixing = "wholesale_fixing";
    public const string FirstCorrectionSettlement = "first_correction_settlement";
    public const string SecondCorrectionSettlement = "second_correction_settlement";
    public const string ThirdCorrectionSettlement = "third_correction_settlement";
}

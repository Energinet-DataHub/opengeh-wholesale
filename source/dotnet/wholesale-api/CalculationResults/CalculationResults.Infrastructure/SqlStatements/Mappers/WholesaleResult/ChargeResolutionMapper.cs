﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.WholesaleResult;

public static class ChargeResolutionMapper
{
    public static ChargeResolution FromDeltaTableValue(string chargeResolution) =>
        chargeResolution switch
        {
            "P1M" => ChargeResolution.Month,
            "P1D" => ChargeResolution.Day,
            "PT1H" => ChargeResolution.Hour,
            _ => throw new ArgumentOutOfRangeException($"Cannot map charge resolution type '{chargeResolution}'"),
        };
}

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

using QuantityQuality = Energinet.DataHub.Wholesale.Contracts.Events.QuantityQuality;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers.CalculationResultCompleted;

public static class QuantityQualityMapper
{
    public static QuantityQuality MapQuantityQuality(CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality quantityQuality)
    {
        return quantityQuality switch
        {
            CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality.Estimated => QuantityQuality.Estimated,
            CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality.Incomplete => QuantityQuality.Incomplete,
            CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality.Measured => QuantityQuality.Measured,
            CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality.Missing => QuantityQuality.Missing,
            CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality.Calculated => QuantityQuality.Calculated,

            _ => throw new ArgumentOutOfRangeException(
                nameof(quantityQuality),
                actualValue: quantityQuality,
                "Value cannot be mapped to a quantity quality."),
        };
    }
}

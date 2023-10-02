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

using Energinet.DataHub.Edi.Responses;

namespace Energinet.DataHub.Wholesale.EDI.Mappers;

public static class QuantityQualityMapper
{
    public static QuantityQuality MapQuantityQuality(Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality quality)
    {
        return quality switch
        {
            Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality.Incomplete => QuantityQuality.Incomplete,
            Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality.Estimated => QuantityQuality.Estimated,
            Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality.Measured => QuantityQuality.Measured,
            Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality.Calculated => QuantityQuality.Calculated,
            Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality.Missing => QuantityQuality.Missing,
            _ => throw new ArgumentOutOfRangeException($"Unknown quality {nameof(quality)}"),
        };
    }
}

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

using EdiModel = Energinet.DataHub.Edi.Responses;
using WholesaleModel = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;

namespace Energinet.DataHub.Wholesale.Edi.Mappers;

public static class QuantityQualityMapper
{
    public static EdiModel.QuantityQuality MapQuantityQuality(WholesaleModel.QuantityQuality quantityQuality)
    {
        return quantityQuality switch
        {
            WholesaleModel.QuantityQuality.Estimated => EdiModel.QuantityQuality.Estimated,
            WholesaleModel.QuantityQuality.Measured => EdiModel.QuantityQuality.Measured,
            WholesaleModel.QuantityQuality.Missing => EdiModel.QuantityQuality.Missing,
            WholesaleModel.QuantityQuality.Calculated => EdiModel.QuantityQuality.Calculated,

            _ => throw new ArgumentOutOfRangeException(
                nameof(quantityQuality),
                actualValue: quantityQuality,
                "Value cannot be mapped to a quantity quality."),
        };
    }
}

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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient.Mappers;

public static class AggregationLevelMapper
{
    public static string ToDeltaTableValue(TimeSeriesType timeSeriesType, string? energySupplierGln, string? balanceResponsiblePartyGln)
    {
        switch (timeSeriesType)
        {
            case TimeSeriesType.NonProfiledConsumption:
            case TimeSeriesType.Production:
            case TimeSeriesType.FlexConsumption:
                if (energySupplierGln != null && balanceResponsiblePartyGln != null)
                    return "es_brp_ga";
                if (energySupplierGln != null)
                    return "es_ga";
                if (balanceResponsiblePartyGln != null)
                    return "brp_ga";
                return "total_ga";
            default:
                throw new NotImplementedException($"Mapping of '{timeSeriesType}' not implemented.");
        }
    }
}

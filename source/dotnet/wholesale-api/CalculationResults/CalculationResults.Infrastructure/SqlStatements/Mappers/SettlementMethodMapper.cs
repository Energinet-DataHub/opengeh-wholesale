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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.EnergyResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;

public static class SettlementMethodMapper
{
    public static SettlementMethod? FromDeltaTableValue(string? settlementMethod) =>
        settlementMethod switch
        {
            "flex" => SettlementMethod.Flex,
            "non_profiled" => SettlementMethod.NonProfiled,
            null => null,

            _ => throw new ArgumentOutOfRangeException(
                nameof(settlementMethod),
                actualValue: settlementMethod,
                "Value does not contain a valid string representation of a settlement method."),
        };

    public static SettlementMethod? FromTimeSeriesTypeDeltaTableValue(string timeSeriesType) =>
        TimeSeriesTypeMapper.FromDeltaTableValue(timeSeriesType) switch
        {
            TimeSeriesType.Production => null,
            TimeSeriesType.FlexConsumption => SettlementMethod.Flex,
            TimeSeriesType.NonProfiledConsumption => SettlementMethod.NonProfiled,
            TimeSeriesType.NetExchangePerGa => null,
            TimeSeriesType.NetExchangePerNeighboringGa => null,
            TimeSeriesType.GridLoss => null,
            TimeSeriesType.NegativeGridLoss => null,
            TimeSeriesType.PositiveGridLoss => SettlementMethod.Flex,
            TimeSeriesType.TotalConsumption => null,
            TimeSeriesType.TempFlexConsumption => null,
            TimeSeriesType.TempProduction => null,

            _ => throw new ArgumentOutOfRangeException(
                nameof(timeSeriesType),
                actualValue: timeSeriesType,
                "Value cannot be mapped to a settlement method."),
        };

    public static string? ToDeltaTableValue(SettlementMethod? settlementMethod)
    {
        return settlementMethod switch
        {
            null => null,
            SettlementMethod.Flex => "flex",
            SettlementMethod.NonProfiled => "non_profiled",

            _ => throw new ArgumentOutOfRangeException(
                nameof(settlementMethod),
                actualValue: settlementMethod,
                "Value does not contain a valid string representation of a settlement method."),
        };
    }
}

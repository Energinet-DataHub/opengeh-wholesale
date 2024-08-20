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

public static class MeteringPointTypeMapper
{
    public static MeteringPointType? FromDeltaTableValue(string? meteringPointType)
    {
        if (meteringPointType == null) return null;

        try
        {
            var normalized = meteringPointType.Replace("_", string.Empty);
            return Enum.Parse<MeteringPointType>(normalized, ignoreCase: true);
        }
        catch (ArgumentException)
        {
            throw new ArgumentOutOfRangeException(
                nameof(meteringPointType),
                actualValue: meteringPointType,
                "Value does not contain a valid string representation of a metering point type.");
        }
    }

    public static MeteringPointType FromDeltaTableValueNonNull(string meteringPointType)
    {
        try
        {
            var normalized = meteringPointType.Replace("_", string.Empty);
            return Enum.Parse<MeteringPointType>(normalized, ignoreCase: true);
        }
        catch (ArgumentException)
        {
            throw new ArgumentOutOfRangeException(
                nameof(meteringPointType),
                actualValue: meteringPointType,
                "Value does not contain a valid string representation of a metering point type.");
        }
    }

    public static MeteringPointType FromTimeSeriesTypeDeltaTableValue(string timeSeriesType) =>
        TimeSeriesTypeMapper.FromDeltaTableValue(timeSeriesType) switch
        {
            TimeSeriesType.Production => MeteringPointType.Production,
            TimeSeriesType.FlexConsumption => MeteringPointType.Consumption,
            TimeSeriesType.NonProfiledConsumption => MeteringPointType.Consumption,
            TimeSeriesType.NetExchangePerNeighboringGa => MeteringPointType.Exchange,
            TimeSeriesType.NetExchangePerGa => MeteringPointType.Exchange,
            TimeSeriesType.GridLoss => MeteringPointType.Consumption,
            TimeSeriesType.NegativeGridLoss => MeteringPointType.Production,
            TimeSeriesType.PositiveGridLoss => MeteringPointType.Consumption,
            TimeSeriesType.TotalConsumption => MeteringPointType.Consumption,
            TimeSeriesType.TempFlexConsumption => MeteringPointType.Consumption,
            TimeSeriesType.TempProduction => MeteringPointType.Production,

            _ => throw new ArgumentOutOfRangeException(
                nameof(timeSeriesType),
                actualValue: timeSeriesType,
                "Value cannot be mapped to a metering point type."),
        };

    public static string ToDeltaTableValue(MeteringPointType meteringPointType)
    {
        return meteringPointType switch
        {
            MeteringPointType.Consumption => "consumption",
            MeteringPointType.Production => "production",
            MeteringPointType.Exchange => "exchange",
            _ => throw new ArgumentOutOfRangeException(nameof(meteringPointType), meteringPointType, null),
        };
    }
}

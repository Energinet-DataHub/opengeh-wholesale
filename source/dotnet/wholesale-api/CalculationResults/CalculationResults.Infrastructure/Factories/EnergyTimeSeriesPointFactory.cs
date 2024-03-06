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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;

public static class EnergyTimeSeriesPointFactory
{
    public static EnergyTimeSeriesPoint CreateTimeSeriesPoint(DatabricksSqlRow databricksSqlRow)
    {
        var time = databricksSqlRow[EnergyResultColumnNames.Time];
        var quantity = databricksSqlRow[EnergyResultColumnNames.Quantity];
        var qualities = databricksSqlRow[EnergyResultColumnNames.QuantityQualities];

        if (qualities == null)
            throw new ArgumentNullException(nameof(qualities));

        return new EnergyTimeSeriesPoint(
            SqlResultValueConverters.ToDateTimeOffset(time)!.Value,
            SqlResultValueConverters.ToDecimal(quantity)!.Value,
            SqlResultValueConverters.ToQuantityQualities(qualities)!);
    }
}

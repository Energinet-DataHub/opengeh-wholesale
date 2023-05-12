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
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient;

public class SqlForBalanceFixingReport
{
    public string CreateSqlForBalanceFixing(
        string[] gridAreaCodes,
        ProcessType processType,
        Instant periodStart,
        Instant periodEnd,
        string? energySupplier)
    {
        return "";
//         return $@"select time, quantity, quantity_quality
// from wholesale_output.result
//   and grid_area = '{gridAreaCode}'
//   and time_series_type = '{ToDeltaValue(timeSeriesType)}'
//   and aggregation_level = '{GetAggregationLevelDeltaValue(timeSeriesType, energySupplierGln, balanceResponsiblePartyGln)}'
// order by time
// "
    }

    public SettlementReportResultRow CreateSettlementReportRow(DatabricksSqlResponse databricksSqlResponse)
    {
        return new SettlementReportResultRow(
            databricksSqlResponse.Rows[],
            databricksSqlResponse.ProcessType,
            databricksSqlResponse.Time,
            databricksSqlResponse.Resolution,
            databricksSqlResponse.MeteringPointType,
            databricksSqlResponse.SettlementMethod,
            databricksSqlResponse.Quantity);
    }
    {

    }

}


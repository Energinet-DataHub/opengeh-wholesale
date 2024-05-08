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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;

public interface ISettlementReportDataRepository
{
    public const string DataSourceUnavailableExceptionMessage = "Call to Databricks SQL Warehouse failed. Is server cold?";

    /// <summary>
    /// Stream the requested data from the data source. If the data source is not ready, an exception is thrown.
    /// </summary>
    IAsyncEnumerable<SettlementReportResultRow> TryReadBalanceFixingResultsAsync(SettlementReportRequestFilterDto filter);
}

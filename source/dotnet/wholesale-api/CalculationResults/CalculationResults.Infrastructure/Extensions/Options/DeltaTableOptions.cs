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

namespace Energinet.DataHub.Wholesale.Common.Infrastructure.Options;

public class DeltaTableOptions
{
    /// <summary>
    /// Name of the schema/database under which the output tables are associated.
    /// </summary>
    public string SCHEMA_NAME { get; set; } = "wholesale_output";

    /// <summary>
    /// Name of the schema/database under which the basis data tables are associated.
    /// </summary>
    public string BasisDataSchemaName { get; set; } = "basis_data";

    /// <summary>
    /// Name of the schema/database under which the settlement report views are associated.
    /// </summary>
    public string SettlementReportSchemaName { get; set; } = "settlement_report";

    /// <summary>
    /// Name of the energy results delta table.
    /// </summary>
    public string ENERGY_RESULTS_TABLE_NAME { get; set; } = "energy_results";

    /// <summary>
    /// Name of the wholesale results delta table.
    /// </summary>
    public string WHOLESALE_RESULTS_TABLE_NAME { get; set; } = "wholesale_results";

    /// <summary>
    /// Name of the wholesale results delta table.
    /// </summary>
    public string TOTAL_MONTHLY_AMOUNTS_TABLE_NAME { get; set; } = "total_monthly_amounts";

    public string WHOLESALE_RESULTS_V1_VIEW_NAME { get; set; } = "wholesale_results_v1";

    public string ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME { get; set; } = "energy_result_points_per_ga_v1";

    public string ENERGY_RESULTS_POINTS_PER_ES_GA_V1_VIEW_NAME { get; set; } = "energy_result_points_per_es_ga_v1";

    public string CalculationResultsSchemaName { get; set; } = "wholesale_calculation_results";

    public string CHARGE_LINK_PERIODS_V1_VIEW_NAME { get; set; } = "charge_link_periods_v1";

    public string METERING_POINT_MASTER_DATA_V1_VIEW_NAME { get; set; } = "metering_point_periods_v1";
}

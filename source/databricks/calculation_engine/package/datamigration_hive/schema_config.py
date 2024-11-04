# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
from spark_sql_migrations import Schema, Table, View

import package.infrastructure.paths as paths
# calculation_input
from package.databases.wholesale_internal.schemas import (
    grid_loss_metering_point_ids_schema,
)

# calculation_output

schema_config = [
    Schema(
        name=paths.HiveOutputDatabase.DATABASE_NAME,
        tables=[],
        views=[
            View(name=paths.HiveOutputDatabase.SUCCEEDED_ENERGY_RESULTS_V1_VIEW_NAME),
        ],
    ),
    Schema(
        # Tables in this schema are externals and schemas are not defined in the SQL scripts.
        # This will be changed to Views in the future.
        name=paths.InputDatabase.DATABASE_NAME,
        tables=[
            Table(
                name=paths.InputDatabase.GRID_LOSS_METERING_POINT_IDS_TABLE_NAME,
                schema=grid_loss_metering_point_ids_schema,
            )
        ],
        views=[],
    ),
    Schema(
        name=paths.HiveSettlementReportPublicDataModel.DATABASE_NAME,
        tables=[],
        views=[
            View(
                name=paths.HiveSettlementReportPublicDataModel.CURRENT_BALANCE_FIXING_CALCULATION_VERSION_VIEW_NAME_V1
            ),
            View(
                name=paths.HiveSettlementReportPublicDataModel.METERING_POINT_PERIODS_VIEW_NAME_V1
            ),
            View(
                name=paths.HiveSettlementReportPublicDataModel.METERING_POINT_TIME_SERIES_VIEW_NAME_V1
            ),
            View(
                name=paths.HiveSettlementReportPublicDataModel.ENERGY_RESULT_POINTS_PER_GA_VIEW_NAME_V1
            ),
            View(
                name=paths.HiveSettlementReportPublicDataModel.ENERGY_RESULT_POINTS_PER_ES_GA_SETTLEMENT_REPORT_VIEW_NAME_V1
            ),
            View(
                name=paths.HiveSettlementReportPublicDataModel.CHARGE_PRICES_VIEW_NAME_V1
            ),
            View(
                name=paths.HiveSettlementReportPublicDataModel.CHARGE_LINK_PERIODS_VIEW_NAME_V1
            ),
            View(
                name=paths.HiveSettlementReportPublicDataModel.MONTHLY_AMOUNTS_VIEW_NAME_V1
            ),
            View(
                name=paths.HiveSettlementReportPublicDataModel.WHOLESALE_RESULTS_VIEW_NAME_V1
            ),
        ],
    ),
    Schema(
        name=paths.CalculationResultsPublicDataModel.DATABASE_NAME,
        tables=[],
        views=[
            View(
                name=paths.CalculationResultsPublicDataModel.ENERGY_RESULT_POINTS_PER_GA_V1_VIEW_NAME
            ),
            View(
                name=paths.CalculationResultsPublicDataModel.ENERGY_RESULT_POINTS_PER_BRP_GA_V1_VIEW_NAME
            ),
            View(
                name=paths.CalculationResultsPublicDataModel.ENERGY_RESULT_POINTS_PER_ES_BRP_GA_V1_VIEW_NAME
            ),
            View(
                name=paths.CalculationResultsPublicDataModel.AMOUNTS_PER_CHARGE_VIEW_NAME
            ),
            View(
                name=paths.CalculationResultsPublicDataModel.MONTHLY_AMOUNTS_PER_CHARGE_VIEW_NAME
            ),
            View(
                name=paths.CalculationResultsPublicDataModel.TOTAL_MONTHLY_AMOUNTS_VIEW_NAME
            ),
            View(
                name=paths.CalculationResultsPublicDataModel.GRID_LOSS_METERING_POINT_TIME_SERIES_VIEW_NAME
            ),
        ],
    ),
]

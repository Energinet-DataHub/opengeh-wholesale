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

import package.calculation.basis_data.schemas as basis_data_schemas
import package.infrastructure.paths as paths

# calculation_input
from package.calculation.input.schemas.grid_loss_metering_points_schema import (
    grid_loss_metering_points_schema,
)
from package.calculation.output.schemas.energy_results_schema import (
    energy_results_schema,
)
from package.calculation.output.schemas.total_monthly_amounts_schema import (
    total_monthly_amounts_schema,
)

# calculation_output
from package.calculation.output.schemas.wholesale_results_schema import (
    wholesale_results_schema,
)

schema_config = [
    Schema(
        name=paths.OUTPUT_DATABASE_NAME,
        tables=[
            Table(
                name=paths.WHOLESALE_RESULT_TABLE_NAME,
                schema=wholesale_results_schema,
            ),
            Table(
                name=paths.ENERGY_RESULT_TABLE_NAME,
                schema=energy_results_schema,
            ),
            Table(
                name=paths.TOTAL_MONTHLY_AMOUNTS_TABLE_NAME,
                schema=total_monthly_amounts_schema,
            ),
        ],
        views=[
            View(name=paths.SUCCEEDED_ENERGY_RESULTS_V1_VIEW_NAME),
        ],
    ),
    Schema(
        # Tables in this schema are externals and schemas are not defined in the SQL scripts.
        # This will be changed to Views in the future.
        name=paths.INPUT_DATABASE_NAME,
        tables=[
            Table(
                name=paths.GRID_LOSS_METERING_POINTS_TABLE_NAME,
                schema=grid_loss_metering_points_schema,
            )
        ],
        views=[],
    ),
    Schema(
        name=paths.BASIS_DATA_DATABASE_NAME,
        tables=[
            Table(
                name=paths.METERING_POINT_PERIODS_BASIS_DATA_TABLE_NAME,
                schema=basis_data_schemas.metering_point_period_schema,
            ),
            Table(
                name=paths.TIME_SERIES_POINTS_BASIS_DATA_TABLE_NAME,
                schema=basis_data_schemas.time_series_point_schema,
            ),
            Table(
                name=paths.CHARGE_LINK_PERIODS_BASIS_DATA_TABLE_NAME,
                schema=basis_data_schemas.charge_link_periods_schema,
            ),
            Table(
                name=paths.CHARGE_MASTER_DATA_PERIODS_BASIS_DATA_TABLE_NAME,
                schema=basis_data_schemas.charge_price_information_periods_schema,
            ),
            Table(
                name=paths.CHARGE_PRICE_POINTS_BASIS_DATA_TABLE_NAME,
                schema=basis_data_schemas.charge_price_points_schema,
            ),
            Table(
                name=paths.GRID_LOSS_METERING_POINTS_TABLE_NAME,
                schema=basis_data_schemas.grid_loss_metering_points_schema,
            ),
            Table(
                name=paths.CALCULATIONS_TABLE_NAME,
                schema=basis_data_schemas.calculations_schema,
            ),
        ],
        views=[],
    ),
    Schema(
        name=paths.SETTLEMENT_REPORT_DATABASE_NAME,
        tables=[],
        views=[
            View(name=paths.LATEST_CALCULATIONS_SETTLEMENT_REPORT_VIEW_NAME_V1),
            View(name=paths.METERING_POINT_PERIODS_SETTLEMENT_REPORT_VIEW_NAME_V1),
            View(name=paths.METERING_POINT_TIME_SERIES_SETTLEMENT_REPORT_VIEW_NAME_V1),
            View(name=paths.ENERGY_RESULTS_SETTLEMENT_REPORT_VIEW_NAME_V1),
            View(name=paths.CHARGE_PRICES_SETTLEMENT_REPORT_VIEW_NAME_V1),
            View(name=paths.CHARGE_LINK_PERIODS_SETTLEMENT_REPORT_VIEW_NAME_V1),
        ],
    ),
    Schema(
        name=paths.EdiResults.DATABASE_NAME,
        tables=[],
        views=[
            View(name=paths.EdiResults.ENERGY_RESULT_POINTS_PER_GA_V1_VIEW_NAME),
        ],
    ),
]

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

from spark_sql_migrations import Schema, Table

import package.calculation.basis_data.schemas as basis_data_schemas

from package.infrastructure.paths import (
    WHOLESALE_RESULT_TABLE_NAME,
    OUTPUT_DATABASE_NAME,
    ENERGY_RESULT_TABLE_NAME,
    INPUT_DATABASE_NAME,
    GRID_LOSS_METERING_POINTS_TABLE_NAME,
    BASIS_DATA_DATABASE_NAME,
    METERING_POINT_PERIODS_TABLE_NAME,
    TIME_SERIES_POINTS_TABLE_NAME,
    CHARGE_LINK_PERIODS_TABLE_NAME,
    CHARGE_MASTER_DATA_PERIODS_TABLE_NAME,
    CHARGE_PRICE_POINTS_TABLE_NAME,
)

# calculation_output
from package.calculation.output.schemas.wholesale_results_schema import (
    wholesale_results_schema,
)
from package.calculation.output.schemas.energy_results_schema import (
    energy_results_schema,
)

# calculation_input
from package.calculation.input.schemas.grid_loss_metering_points_schema import (
    grid_loss_metering_points_schema,
)

schema_config = [
    Schema(
        name=OUTPUT_DATABASE_NAME,
        tables=[
            Table(
                name=WHOLESALE_RESULT_TABLE_NAME,
                schema=wholesale_results_schema,
            ),
            Table(
                name=ENERGY_RESULT_TABLE_NAME,
                schema=energy_results_schema,
            ),
        ],
    ),
    Schema(
        # Tables in this schema are externals and schemas are not defined in the SQL scripts.
        # This will be changed to Views in the future.
        name=INPUT_DATABASE_NAME,
        tables=[
            Table(
                name=GRID_LOSS_METERING_POINTS_TABLE_NAME,
                schema=grid_loss_metering_points_schema,
            )
        ],
    ),
    Schema(
        name=BASIS_DATA_DATABASE_NAME,
        tables=[
            Table(
                name=METERING_POINT_PERIODS_TABLE_NAME,
                schema=basis_data_schemas.metering_point_period_schema,
            ),
            Table(
                name=TIME_SERIES_POINTS_TABLE_NAME,
                schema=basis_data_schemas.time_series_point_schema,
            ),
            Table(
                name=CHARGE_LINK_PERIODS_TABLE_NAME,
                schema=basis_data_schemas.charge_link_periods_schema,
            ),
            Table(
                name=CHARGE_MASTER_DATA_PERIODS_TABLE_NAME,
                schema=basis_data_schemas.charge_master_data_periods_schema,
            ),
            Table(
                name=CHARGE_PRICE_POINTS_TABLE_NAME,
                schema=basis_data_schemas.charge_price_points_schema,
            ),
            Table(
                name=GRID_LOSS_METERING_POINTS_TABLE_NAME,
                schema=basis_data_schemas.grid_loss_metering_points_schema,
            ),
        ],
    ),
]

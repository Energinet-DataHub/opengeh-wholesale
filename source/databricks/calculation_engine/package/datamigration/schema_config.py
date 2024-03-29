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
import package.infrastructure.paths as paths

# calculation_input
from package.calculation.input.schemas.grid_loss_metering_points_schema import (
    grid_loss_metering_points_schema,
)
from package.calculation.output.schemas.energy_results_schema import (
    energy_results_schema,
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
    ),
    Schema(
        name=paths.BASIS_DATA_DATABASE_NAME,
        tables=[
            Table(
                name=paths.METERING_POINT_PERIODS_TABLE_NAME,
                schema=basis_data_schemas.metering_point_period_schema,
            ),
            Table(
                name=paths.TIME_SERIES_POINTS_TABLE_NAME,
                schema=basis_data_schemas.time_series_point_schema,
            ),
            Table(
                name=paths.CHARGE_LINK_PERIODS_TABLE_NAME,
                schema=basis_data_schemas.charge_link_periods_schema,
            ),
            Table(
                name=paths.CHARGE_MASTER_DATA_PERIODS_TABLE_NAME,
                schema=basis_data_schemas.charge_master_data_periods_schema,
            ),
            Table(
                name=paths.CHARGE_PRICE_POINTS_TABLE_NAME,
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
    ),
]

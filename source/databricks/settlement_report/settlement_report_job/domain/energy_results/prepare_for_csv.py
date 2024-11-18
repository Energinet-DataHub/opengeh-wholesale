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

from pyspark.sql import DataFrame, functions as F

from telemetry_logging import Logger, use_span
from settlement_report_job.domain.utils.csv_column_names import (
    CsvColumnNames,
    EphemeralColumns,
)
from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.domain.utils.map_from_dict import (
    map_from_dict,
)
from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)
import settlement_report_job.domain.utils.map_to_csv_naming as market_naming

log = Logger(__name__)


@use_span()
def prepare_for_csv(
    energy: DataFrame,
    one_file_per_grid_area: bool,
    requesting_actor_market_role: MarketRole,
) -> DataFrame:
    select_columns = [
        F.col(DataProductColumnNames.grid_area_code).alias(
            CsvColumnNames.grid_area_code
        ),
        map_from_dict(market_naming.CALCULATION_TYPES_TO_ENERGY_BUSINESS_PROCESS)[
            F.col(DataProductColumnNames.calculation_type)
        ].alias(CsvColumnNames.calculation_type),
        F.col(DataProductColumnNames.time).alias(CsvColumnNames.time),
        F.col(DataProductColumnNames.resolution).alias(CsvColumnNames.resolution),
        map_from_dict(market_naming.METERING_POINT_TYPES)[
            F.col(DataProductColumnNames.metering_point_type)
        ].alias(CsvColumnNames.metering_point_type),
        map_from_dict(market_naming.SETTLEMENT_METHODS)[
            F.col(DataProductColumnNames.settlement_method)
        ].alias(CsvColumnNames.settlement_method),
        F.col(DataProductColumnNames.quantity).alias(CsvColumnNames.energy_quantity),
    ]

    if requesting_actor_market_role is MarketRole.DATAHUB_ADMINISTRATOR:
        select_columns.insert(
            1,
            F.col(DataProductColumnNames.energy_supplier_id).alias(
                CsvColumnNames.energy_supplier_id
            ),
        )

    if one_file_per_grid_area:
        select_columns.append(
            F.col(DataProductColumnNames.grid_area_code).alias(
                EphemeralColumns.grid_area_code_partitioning
            ),
        )

    return energy.select(select_columns)

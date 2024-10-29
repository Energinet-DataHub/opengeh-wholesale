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

from pyspark.sql import DataFrame, functions as F, Window

from settlement_report_job import logging
from settlement_report_job.domain.csv_column_names import (
    CsvColumnNames,
    EphemeralColumns,
)
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.utils import (
    map_from_dict,
)
from settlement_report_job.wholesale.column_names import DataProductColumnNames
import settlement_report_job.domain.report_naming_convention as market_naming

log = logging.Logger(__name__)


@logging.use_span()
def prepare_for_csv(
    energy: DataFrame,
    create_ephemeral_grid_area_column: bool,
    requesting_actor_market_role: MarketRole,
) -> DataFrame:
    select_columns = [
        F.col(DataProductColumnNames.grid_area_code).alias(
            CsvColumnNames.metering_grid_area
        ),
        map_from_dict(market_naming.CALCULATION_TYPES_TO_ENERGY_BUSINESS_PROCESS)[
            F.col(DataProductColumnNames.calculation_type)
        ].alias(CsvColumnNames.energy_business_process),
        F.col(DataProductColumnNames.time).alias(CsvColumnNames.start_date_time),
        F.col(DataProductColumnNames.resolution).alias(
            CsvColumnNames.resolution_duration
        ),
        map_from_dict(market_naming.METERING_POINT_TYPES)[
            F.col(DataProductColumnNames.metering_point_type)
        ].alias(CsvColumnNames.type_of_mp),
        map_from_dict(market_naming.SETTLEMENT_METHODS)[
            F.col(DataProductColumnNames.settlement_method)
        ].alias(CsvColumnNames.settlement_method),
        F.col(DataProductColumnNames.quantity).alias(CsvColumnNames.energy_quantity),
    ]

    if requesting_actor_market_role not in [
        MarketRole.GRID_ACCESS_PROVIDER,
        MarketRole.ENERGY_SUPPLIER,
    ]:
        select_columns.insert(
            1,
            F.col(DataProductColumnNames.energy_supplier_id).alias(
                CsvColumnNames.energy_supplier_id
            ),
        )

    if create_ephemeral_grid_area_column:
        select_columns.append(
            F.col(DataProductColumnNames.grid_area_code).alias(
                EphemeralColumns.grid_area_code
            ),
        )

    return energy.select(select_columns)

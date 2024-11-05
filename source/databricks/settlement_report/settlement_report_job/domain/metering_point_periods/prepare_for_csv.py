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

from settlement_report_job import logging
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.report_naming_convention import (
    METERING_POINT_TYPES,
    SETTLEMENT_METHODS,
)
from settlement_report_job.domain.csv_column_names import (
    CsvColumnNames,
    EphemeralColumns,
)
from settlement_report_job.utils import map_from_dict
from settlement_report_job.wholesale.column_names import DataProductColumnNames

log = logging.Logger(__name__)


@logging.use_span()
def prepare_for_csv(
    metering_point_periods: DataFrame,
    requesting_actor_market_role: MarketRole,
) -> DataFrame:

    columns = [
        F.col(DataProductColumnNames.grid_area_code).alias(
            EphemeralColumns.grid_area_code_partitioning
        ),
        F.col(DataProductColumnNames.metering_point_id).alias(
            CsvColumnNames.metering_point_id
        ),
        F.col(DataProductColumnNames.from_date).alias(
            CsvColumnNames.metering_point_from_date
        ),
        F.col(DataProductColumnNames.to_date).alias(
            CsvColumnNames.metering_point_to_date
        ),
        F.col(DataProductColumnNames.grid_area_code).alias(
            CsvColumnNames.grid_area_code
        ),
        F.col(DataProductColumnNames.to_grid_area_code).alias(
            CsvColumnNames.to_grid_area_code
        ),
        F.col(DataProductColumnNames.from_grid_area_code).alias(
            CsvColumnNames.from_grid_area_code
        ),
        map_from_dict(METERING_POINT_TYPES)[
            F.col(DataProductColumnNames.metering_point_type)
        ].alias(CsvColumnNames.metering_point_type),
        map_from_dict(SETTLEMENT_METHODS)[
            F.col(DataProductColumnNames.settlement_method)
        ].alias(CsvColumnNames.settlement_method),
    ]

    if requesting_actor_market_role in [
        MarketRole.SYSTEM_OPERATOR,
        MarketRole.DATAHUB_ADMINISTRATOR,
    ]:
        columns.append(
            F.col(DataProductColumnNames.energy_supplier_id).alias(
                CsvColumnNames.energy_supplier_id
            )
        )

    csv_df = metering_point_periods.select(columns)

    return csv_df

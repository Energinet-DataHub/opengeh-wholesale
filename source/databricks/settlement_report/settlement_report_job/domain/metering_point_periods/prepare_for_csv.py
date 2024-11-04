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
    charge_link_periods: DataFrame,
    requesting_actor_market_role: MarketRole,
) -> DataFrame:
    csv_df = charge_link_periods.select(
        F.col(DataProductColumnNames.grid_area_code).alias(
            EphemeralColumns.grid_area_code_partitioning
        ),
        F.col(DataProductColumnNames.metering_point_id).alias(
            CsvColumnNames.metering_point_id
        ),
        map_from_dict(METERING_POINT_TYPES)[
            F.col(DataProductColumnNames.metering_point_type)
        ].alias(CsvColumnNames.metering_point_type),
        F.col(DataProductColumnNames.charge_type).alias(CsvColumnNames.charge_type),
        F.col(DataProductColumnNames.charge_owner_id).alias(
            CsvColumnNames.charge_owner_id
        ),
        F.col(DataProductColumnNames.charge_code).alias(CsvColumnNames.charge_code),
        F.col(DataProductColumnNames.quantity).alias(CsvColumnNames.charge_quantity),
        F.col(DataProductColumnNames.from_date).alias(
            CsvColumnNames.charge_link_from_date
        ),
        F.col(DataProductColumnNames.to_date).alias(CsvColumnNames.charge_link_to_date),
        F.col(DataProductColumnNames.energy_supplier_id).alias(
            CsvColumnNames.energy_supplier_id
        ),
    )

    if requesting_actor_market_role in [
        MarketRole.GRID_ACCESS_PROVIDER,
        MarketRole.ENERGY_SUPPLIER,
    ]:
        csv_df = csv_df.drop(CsvColumnNames.energy_supplier_id)

    has_energy_supplier_id_column = CsvColumnNames.energy_supplier_id in csv_df.columns

    return csv_df.orderBy(_get_order_by_columns(has_energy_supplier_id_column))


def _get_order_by_columns(
    has_energy_supplier_id_column: bool,
) -> list[str]:

    order_by_columns = [
        CsvColumnNames.metering_point_type,
        CsvColumnNames.metering_point_id,
        CsvColumnNames.charge_owner_id,
        CsvColumnNames.charge_code,
        CsvColumnNames.charge_link_from_date,
    ]
    if has_energy_supplier_id_column:
        order_by_columns.insert(0, CsvColumnNames.energy_supplier_id)

    return order_by_columns

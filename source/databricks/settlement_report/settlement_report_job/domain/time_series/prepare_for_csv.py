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
from settlement_report_job.domain.dataframe_utils.get_start_of_day import (
    get_start_of_day,
)
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.report_naming_convention import (
    METERING_POINT_TYPES,
)
from settlement_report_job.domain.csv_column_names import (
    CsvColumnNames,
)
from settlement_report_job.utils import (
    map_from_dict,
)
from settlement_report_job.wholesale.column_names import DataProductColumnNames
from settlement_report_job.wholesale.data_values import (
    MeteringPointResolutionDataProductValue,
)

log = logging.Logger(__name__)


@logging.use_span()
def prepare_for_csv(
    filtered_time_series_points: DataFrame,
    metering_point_resolution: MeteringPointResolutionDataProductValue,
    time_zone: str,
    requesting_actor_market_role: MarketRole,
) -> DataFrame:
    desired_number_of_quantity_columns = _get_desired_quantity_column_count(
        metering_point_resolution
    )

    filtered_time_series_points = filtered_time_series_points.withColumn(
        CsvColumnNames.time,
        get_start_of_day(DataProductColumnNames.observation_time, time_zone),
    )

    win = Window.partitionBy(
        DataProductColumnNames.grid_area_code,
        DataProductColumnNames.energy_supplier_id,
        DataProductColumnNames.metering_point_id,
        DataProductColumnNames.metering_point_type,
        CsvColumnNames.time,
    ).orderBy(DataProductColumnNames.observation_time)
    filtered_time_series_points = filtered_time_series_points.withColumn(
        "chronological_order", F.row_number().over(win)
    )

    pivoted_df = (
        filtered_time_series_points.groupBy(
            DataProductColumnNames.grid_area_code,
            DataProductColumnNames.energy_supplier_id,
            DataProductColumnNames.metering_point_id,
            DataProductColumnNames.metering_point_type,
            CsvColumnNames.time,
        )
        .pivot(
            "chronological_order",
            list(range(1, desired_number_of_quantity_columns + 1)),
        )
        .agg(F.first(DataProductColumnNames.quantity))
    )

    quantity_column_names = [
        F.col(str(i)).alias(f"{CsvColumnNames.quantity}{i}")
        for i in range(1, desired_number_of_quantity_columns + 1)
    ]

    csv_df = pivoted_df.select(
        F.col(DataProductColumnNames.energy_supplier_id).alias(
            CsvColumnNames.energy_supplier_id
        ),
        F.col(DataProductColumnNames.grid_area_code).alias(
            CsvColumnNames.grid_area_code
        ),
        F.col(DataProductColumnNames.metering_point_id).alias(
            CsvColumnNames.metering_point_id
        ),
        map_from_dict(METERING_POINT_TYPES)[
            F.col(DataProductColumnNames.metering_point_type)
        ].alias(CsvColumnNames.metering_point_type),
        F.col(CsvColumnNames.time),
        *quantity_column_names,
    )

    if requesting_actor_market_role in [
        MarketRole.GRID_ACCESS_PROVIDER,
        MarketRole.ENERGY_SUPPLIER,
    ]:
        csv_df = csv_df.drop(CsvColumnNames.energy_supplier_id)

    has_energy_supplier_id_column = CsvColumnNames.energy_supplier_id in csv_df.columns

    return csv_df.orderBy(_get_order_by_columns(has_energy_supplier_id_column))


def _get_desired_quantity_column_count(
    resolution: MeteringPointResolutionDataProductValue,
) -> int:
    if resolution == MeteringPointResolutionDataProductValue.HOUR:
        return 25
    elif resolution == MeteringPointResolutionDataProductValue.QUARTER:
        return 25 * 4
    else:
        raise ValueError(f"Unknown time series resolution: {resolution}")


def _get_order_by_columns(
    has_energy_supplier_id_column: bool,
) -> list[str]:

    order_by_columns = [
        CsvColumnNames.type_of_mp,
        CsvColumnNames.metering_point_id,
        CsvColumnNames.start_date_time,
    ]
    if has_energy_supplier_id_column:
        order_by_columns.insert(0, CsvColumnNames.energy_supplier_id)

    return order_by_columns

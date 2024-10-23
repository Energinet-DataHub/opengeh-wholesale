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
from datetime import datetime
from uuid import UUID

from pyspark.sql import DataFrame, functions as F, Column, Window

from settlement_report_job import logging
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.wholesale.column_names import DataProductColumnNames

log = logging.Logger(__name__)


@logging.use_span()
def read_and_filter(
    period_start: datetime,
    period_end: datetime,
    calculation_id_by_grid_area: dict[str, UUID],
    energy_supplier_ids: list[str] | None,
    requesting_actor_market_role: MarketRole,
    requesting_actor_id: str,
    repository: WholesaleRepository,
) -> DataFrame:
    log.info("Creating charge links")

    metering_point_periods = _read_metering_point_periods(
        period_start=period_start,
        period_end=period_end,
        energy_supplier_ids=energy_supplier_ids,
        repository=repository,
    )
    charge_link_periods = _read_charge_link_periods(
        period_start=period_start,
        period_end=period_end,
        requesting_actor_id=requesting_actor_id,
        requesting_actor_market_role=requesting_actor_market_role,
        repository=repository,
    )

    metering_point_periods = metering_point_periods.where(
        _filter_on_calculation_id_by_grid_area(calculation_id_by_grid_area)
    )

    metering_point_periods.show()
    charge_link_periods.show()

    link_from_date = "link_from_date"
    link_to_date = "link_to_date"
    metering_point_from_date = "metering_point_from_date"
    metering_point_to_date = "metering_point_to_date"

    charge_link_periods = charge_link_periods.withColumnRenamed(
        DataProductColumnNames.from_date, link_from_date
    ).withColumnRenamed(DataProductColumnNames.to_date, link_to_date)
    metering_point_periods = metering_point_periods.withColumnRenamed(
        DataProductColumnNames.from_date, metering_point_from_date
    ).withColumnRenamed(DataProductColumnNames.to_date, metering_point_to_date)

    charge_link_periods = (
        charge_link_periods.join(
            metering_point_periods,
            on=[
                DataProductColumnNames.calculation_id,
                DataProductColumnNames.metering_point_id,
            ],
            how="inner",
        )
        .where(
            (F.col(link_from_date) < F.col(metering_point_to_date))
            & (F.col(link_to_date) > F.col(metering_point_from_date))
        )
        .select(
            DataProductColumnNames.metering_point_id,
            DataProductColumnNames.metering_point_type,
            DataProductColumnNames.charge_type,
            DataProductColumnNames.charge_code,
            DataProductColumnNames.charge_owner_id,
            DataProductColumnNames.quantity,
            F.greatest(F.col(link_from_date), F.col(metering_point_from_date)).alias(
                DataProductColumnNames.from_date
            ),
            F.least(F.col(link_to_date), F.col(metering_point_to_date)).alias(
                DataProductColumnNames.to_date
            ),
            DataProductColumnNames.grid_area_code,
            DataProductColumnNames.energy_supplier_id,
        )
        .distinct()
    )

    return charge_link_periods


def _read_metering_point_periods(
    period_start: datetime,
    period_end: datetime,
    energy_supplier_ids: list[str] | None,
    repository: WholesaleRepository,
) -> DataFrame:

    metering_point_periods = repository.read_metering_point_periods().where(
        (F.col(DataProductColumnNames.from_date) < period_end)
        & (F.col(DataProductColumnNames.to_date) > period_start)
    )
    if energy_supplier_ids is not None:
        metering_point_periods = metering_point_periods.where(
            F.col(DataProductColumnNames.energy_supplier_id).isin(energy_supplier_ids)
        )

    return metering_point_periods


def _read_charge_link_periods(
    period_start: datetime,
    period_end: datetime,
    requesting_actor_id: str,
    requesting_actor_market_role: MarketRole,
    repository: WholesaleRepository,
) -> DataFrame:
    charge_link_periods = repository.read_charge_link_periods().where(
        (F.col(DataProductColumnNames.from_date) < period_end)
        & (F.col(DataProductColumnNames.to_date) > period_start)
    )

    if requesting_actor_market_role in [
        MarketRole.SYSTEM_OPERATOR,
        MarketRole.GRID_ACCESS_PROVIDER,
    ]:
        charge_price_information_periods = (
            repository.read_charge_price_information_periods()
        )

        is_tax = (
            False
            if requesting_actor_market_role == MarketRole.SYSTEM_OPERATOR
            else True
        )

        charge_link_periods = filter_on_charge_owner_and_tax(
            charge_link_periods=charge_link_periods,
            charge_price_information_periods=charge_price_information_periods,
            charge_owner_id=requesting_actor_id,
            is_tax=is_tax,
        )

    return charge_link_periods


def _filter_on_calculation_id_by_grid_area(
    calculation_id_by_grid_area: dict[str, UUID],
) -> Column:
    calculation_id_by_grid_area_structs = [
        F.struct(F.lit(grid_area_code), F.lit(str(calculation_id)))
        for grid_area_code, calculation_id in calculation_id_by_grid_area.items()
    ]

    return F.struct(
        F.col(DataProductColumnNames.grid_area_code),
        F.col(DataProductColumnNames.calculation_id),
    ).isin(calculation_id_by_grid_area_structs)


def filter_on_charge_owner_and_tax(
    charge_link_periods: DataFrame,
    charge_price_information_periods: DataFrame,
    charge_owner_id: str,
    is_tax: bool,
) -> DataFrame:
    charge_link_periods = charge_link_periods.join(
        charge_price_information_periods,
        on=[DataProductColumnNames.calculation_id, DataProductColumnNames.charge_key],
        how="inner",
    ).select(
        charge_link_periods["*"],
        charge_price_information_periods[DataProductColumnNames.is_tax],
    )

    return charge_link_periods.where(
        (F.col(DataProductColumnNames.charge_owner_id) == charge_owner_id)
        & (F.col(DataProductColumnNames.is_tax) == F.lit(is_tax))
    )


def merge_connected_periods(
    input_df: DataFrame, group_by_columns: list[str]
) -> DataFrame:

    # Define the window specification
    window_spec = Window.partitionBy(group_by_columns).orderBy(
        DataProductColumnNames.from_date
    )

    # Add columns to identify overlapping periods
    df_with_next = input_df.withColumn(
        "next_from_date", F.lead(DataProductColumnNames.from_date).over(window_spec)
    ).withColumn(
        "next_to_date", F.lead(DataProductColumnNames.to_date).over(window_spec)
    )

    print("df_with_next")
    df_with_next.show()

    # Merge overlapping periods
    df = df_with_next.withColumn(
        "merged_from_date",
        F.when(
            F.col(DataProductColumnNames.to_date) >= F.col("next_from_date"),
            F.least(F.col(DataProductColumnNames.from_date), F.col("next_from_date")),
        ).otherwise(F.col(DataProductColumnNames.from_date)),
    ).withColumn(
        "merged_to_date",
        F.when(
            F.col(DataProductColumnNames.to_date) >= F.col("next_from_date"),
            F.greatest(F.col(DataProductColumnNames.to_date), F.col("next_to_date")),
        ).otherwise(F.col(DataProductColumnNames.to_date)),
    )

    # Select the merged periods
    merged_df = df.groupBy(group_by_columns).agg(
        F.min("merged_from_date").alias(DataProductColumnNames.from_date),
        F.max("merged_to_date").alias(DataProductColumnNames.to_date),
    )

    print(*group_by_columns)

    # Include non-overlapping periods
    non_overlapping_df = df_with_next.filter(
        (F.col(DataProductColumnNames.to_date) < F.col("next_from_date"))
        | F.col("next_from_date").isNull()
    )

    non_overlapping_df = non_overlapping_df.select(
        "charge_key", DataProductColumnNames.from_date, DataProductColumnNames.to_date
    )

    # Union merged and non-overlapping periods
    result_df = merged_df.union(non_overlapping_df).distinct()
    return result_df


def merge_connected_periods_2(
    input_df: DataFrame, group_by_columns: list[str]
) -> DataFrame:
    # Self-join on charge_key and overlapping date ranges
    joined_df = (
        input_df.alias("df1")
        .join(
            input_df.alias("df2"),
            (F.col("df1.charge_key") == F.col("df2.charge_key"))
            & (F.col("df1.from_date") <= F.col("df2.to_date"))
            & (F.col("df1.to_date") >= F.col("df2.from_date"))
            & (F.col("df1.from_date") != F.col("df2.from_date"))
            & (F.col("df1.to_date") != F.col("df2.to_date")),
            "left",
        )
        .where(F.col("df2.charge_key").isNotNull())
    )

    # Merge overlapping periods
    merged_df = joined_df.groupBy("df1.charge_key").agg(
        F.min(F.least(F.col("df1.from_date"), F.col("df2.from_date"))).alias(
            "from_date"
        ),
        F.max(F.greatest(F.col("df1.to_date"), F.col("df2.to_date"))).alias("to_date"),
    )

    # Include non-overlapping periods
    non_overlapping_df = (
        input_df.alias("df1")
        .join(
            merged_df.alias("df2"),
            (F.col("df1.charge_key") == F.col("df2.charge_key"))
            & (
                (
                    (F.col("df1.from_date") > F.col("df2.from_date"))
                    & (F.col("df1.from_date") < F.col("df2.to_date"))
                )
                | (
                    (F.col("df1.to_date") > F.col("df2.from_date"))
                    & (F.col("df1.to_date") < F.col("df2.to_date"))
                )
            ),
            "left_anti",
        )
        .select("df1.*")
    )

    # Union merged and non-overlapping periods
    result_df = merged_df.union(non_overlapping_df).distinct()
    return result_df

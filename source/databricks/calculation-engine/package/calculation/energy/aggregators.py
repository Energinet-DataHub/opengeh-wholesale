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

from decimal import Decimal

from package.codelists import (
    MeteringPointType,
    SettlementMethod,
    QuantityQuality,
)
from package.constants import Colname
from . import transformations as T
from pyspark.sql import DataFrame
from pyspark.sql.functions import (
    col,
    lit,
    row_number,
    when,
)
from pyspark.sql.types import StringType
from pyspark.sql.window import Window
from typing import Union


def aggregate_non_profiled_consumption_ga_brp_es(
    enriched_time_series: DataFrame,
) -> DataFrame:
    return _aggregate_per_ga_and_brp_and_es(
        enriched_time_series,
        MeteringPointType.CONSUMPTION,
        SettlementMethod.NON_PROFILED,
    )


def aggregate_flex_consumption_ga_brp_es(enriched_time_series: DataFrame) -> DataFrame:
    return _aggregate_per_ga_and_brp_and_es(
        enriched_time_series,
        MeteringPointType.CONSUMPTION,
        SettlementMethod.FLEX,
    )


def aggregate_production_ga_brp_es(enriched_time_series: DataFrame) -> DataFrame:
    return _aggregate_per_ga_and_brp_and_es(
        enriched_time_series, MeteringPointType.PRODUCTION, None
    )


def _aggregate_per_ga_and_brp_and_es(
    df: DataFrame,
    market_evaluation_point_type: MeteringPointType,
    settlement_method: Union[SettlementMethod, None],
) -> DataFrame:
    """This function creates a intermediate result, which is subsequently used as input to achieve result for different process steps.

    The function is responsible for
    - Converting hour data to quarter data.
    - Sum quantities across metering points per grid area, energy supplier, and balance responsible.
    - Assign quality when performing sum.

    Each row in the output dataframe corresponds to a unique combination of: ga, brp, es, and quarter_time

    """

    result = df.filter(
        col(Colname.metering_point_type) == market_evaluation_point_type.value
    )
    if settlement_method is not None:
        result = result.filter(
            col(Colname.settlement_method) == settlement_method.value
        )

    sum_group_by = [
        Colname.grid_area,
        Colname.balance_responsible_id,
        Colname.energy_supplier_id,
        Colname.time_window,
    ]
    result = T.aggregate_sum_and_set_quality(result, "quarter_quantity", sum_group_by)

    result = (
        result.withColumn(
            Colname.sum_quantity,
            when(col(Colname.sum_quantity).isNull(), Decimal("0.000")).otherwise(
                col(Colname.sum_quantity)
            ),
        )
        .withColumn(
            Colname.quality,
            when(
                col(Colname.quality).isNull(), QuantityQuality.MISSING.value
            ).otherwise(col(Colname.quality)),
        )
        .select(
            Colname.grid_area,
            Colname.balance_responsible_id,
            Colname.energy_supplier_id,
            Colname.time_window,
            Colname.quality,
            Colname.sum_quantity,
            lit(market_evaluation_point_type.value).alias(Colname.metering_point_type),
            lit(None if settlement_method is None else settlement_method.value)
            .cast(StringType())
            .alias(Colname.settlement_method),
        )
    )

    return T.create_dataframe_from_aggregation_result_schema(result)


def aggregate_production_ga_es(production: DataFrame) -> DataFrame:
    return _aggregate_per_ga_and_es(
        production,
        MeteringPointType.PRODUCTION,
    )


def aggregate_non_profiled_consumption_ga_es(
    non_profiled_consumption: DataFrame,
) -> DataFrame:
    return _aggregate_per_ga_and_es(
        non_profiled_consumption,
        MeteringPointType.CONSUMPTION,
    )


def aggregate_flex_consumption_ga_es(flex_consumption: DataFrame) -> DataFrame:
    return _aggregate_per_ga_and_es(
        flex_consumption,
        MeteringPointType.CONSUMPTION,
    )


def _aggregate_per_ga_and_es(
    df: DataFrame, market_evaluation_point_type: MeteringPointType
) -> DataFrame:
    group_by = [Colname.grid_area, Colname.energy_supplier_id, Colname.time_window]
    result = T.aggregate_sum_and_set_quality(df, Colname.sum_quantity, group_by)

    result = result.select(
        Colname.grid_area,
        Colname.energy_supplier_id,
        Colname.time_window,
        Colname.quality,
        Colname.sum_quantity,
        lit(market_evaluation_point_type.value).alias(Colname.metering_point_type),
    )
    return T.create_dataframe_from_aggregation_result_schema(result)


def aggregate_production_ga_brp(production: DataFrame) -> DataFrame:
    return _aggregate_per_ga_and_brp(production, MeteringPointType.PRODUCTION)


def aggregate_non_profiled_consumption_ga_brp(
    non_profiled_consumption: DataFrame,
) -> DataFrame:
    return _aggregate_per_ga_and_brp(
        non_profiled_consumption,
        MeteringPointType.CONSUMPTION,
    )


def aggregate_flex_consumption_ga_brp(flex_consumption: DataFrame) -> DataFrame:
    return _aggregate_per_ga_and_brp(
        flex_consumption,
        MeteringPointType.CONSUMPTION,
    )


# Function to aggregate sum per grid area and balance responsible party
def _aggregate_per_ga_and_brp(
    df: DataFrame,
    market_evaluation_point_type: MeteringPointType,
) -> DataFrame:
    group_by = [Colname.grid_area, Colname.balance_responsible_id, Colname.time_window]
    result = T.aggregate_sum_and_set_quality(df, Colname.sum_quantity, group_by)

    result = result.select(
        Colname.grid_area,
        Colname.balance_responsible_id,
        Colname.time_window,
        Colname.quality,
        Colname.sum_quantity,
        lit(market_evaluation_point_type.value).alias(Colname.metering_point_type),
    )
    return T.create_dataframe_from_aggregation_result_schema(result)


def aggregate_production_ga(production: DataFrame) -> DataFrame:
    return _aggregate_per_ga(
        production,
        MeteringPointType.PRODUCTION,
    )


def aggregate_non_profiled_consumption_ga(consumption: DataFrame) -> DataFrame:
    return _aggregate_per_ga(
        consumption,
        MeteringPointType.CONSUMPTION,
    )


def aggregate_flex_consumption_ga(
    flex_consumption: DataFrame,
) -> DataFrame:
    return _aggregate_per_ga(
        flex_consumption,
        MeteringPointType.CONSUMPTION,
    )


# Function to aggregate sum per grid area
def _aggregate_per_ga(
    df: DataFrame,
    market_evaluation_point_type: MeteringPointType,
) -> DataFrame:
    group_by = [Colname.grid_area, Colname.time_window]
    result = T.aggregate_sum_and_set_quality(df, Colname.sum_quantity, group_by)

    result = result.withColumnRenamed(
        f"sum({Colname.sum_quantity})", Colname.sum_quantity
    ).select(
        Colname.grid_area,
        Colname.time_window,
        Colname.quality,
        Colname.sum_quantity,
        lit(market_evaluation_point_type.value).alias(Colname.metering_point_type),
    )

    return T.create_dataframe_from_aggregation_result_schema(result)

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


from pyspark.sql import DataFrame
from pyspark.sql.functions import (
    col,
    year,
    month,
    count,
    first,
    sum,
    lit,
    collect_set,
    flatten,
    min,
    to_date,
    from_utc_timestamp,
)
import package.calculation.wholesale.wholesale_initializer as init
from package.calculation.wholesale.tariff_calculators import (
    calculate_tariff_price_per_ga_co_es,
)
from package.codelists import ChargeResolution, MeteringPointType
from package.constants import Colname
from package.calculation_input import CalculationInputReader
from package.calculation_output.wholesale_calculation_result_writer import (
    WholesaleCalculationResultWriter,
)
from .charges_reader import read_charges


def execute(
    calculation_input_reader: CalculationInputReader,
    wholesale_calculation_result_writer: WholesaleCalculationResultWriter,
    metering_points_periods_df: DataFrame,  # TODO: use enriched_time_series
    time_series_point_df: DataFrame,  # TODO: use enriched_time_series
    time_zone: str,
) -> None:
    # Get input data
    metering_points_periods_df = _get_production_and_consumption_metering_points(
        metering_points_periods_df
    )
    charges_df = read_charges(calculation_input_reader)

    # Calculate and write to storage
    _calculate_tariff_charges(
        wholesale_calculation_result_writer,
        metering_points_periods_df,
        time_series_point_df,
        charges_df,
        time_zone,
    )


def _calculate_tariff_charges(
    wholesale_calculation_result_writer: WholesaleCalculationResultWriter,
    metering_points_periods_df: DataFrame,
    time_series_point_df: DataFrame,
    charges_df: DataFrame,
    time_zone: str,
) -> None:
    tariffs_hourly = init.get_tariff_charges(
        metering_points_periods_df,
        time_series_point_df,
        charges_df,
        ChargeResolution.HOUR,
    )

    hourly_tariff_per_ga_co_es = calculate_tariff_price_per_ga_co_es(tariffs_hourly)
    wholesale_calculation_result_writer.write(hourly_tariff_per_ga_co_es)

    monthly_tariff_per_ga_co_es = sum_within_month(
        hourly_tariff_per_ga_co_es, time_zone
    )
    wholesale_calculation_result_writer.write(monthly_tariff_per_ga_co_es)


def _get_production_and_consumption_metering_points(
    metering_points_periods_df: DataFrame,
) -> DataFrame:
    return metering_points_periods_df.filter(
        (col(Colname.metering_point_type) == MeteringPointType.CONSUMPTION.value)
        | (col(Colname.metering_point_type) == MeteringPointType.PRODUCTION.value)
    )


def sum_within_month(df: DataFrame, time_zone: str) -> DataFrame:
    df = df.withColumn(
        Colname.local_date,
        to_date(from_utc_timestamp(col(Colname.observation_time), time_zone)),
    )
    df = df.withColumn("year", year(df[Colname.local_date]))
    df = df.withColumn("month", month(df[Colname.local_date]))
    agg_df = (
        df.groupBy(
            Colname.energy_supplier_id,
            Colname.grid_area,
            "year",
            "month",
            Colname.charge_key,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
        )
        .agg(
            sum(Colname.total_amount).alias(Colname.total_amount),
            sum(Colname.total_quantity).alias(Colname.total_quantity),
            sum(Colname.charge_price).alias(Colname.charge_price),
            first(Colname.charge_tax).alias(Colname.charge_tax),
            first(Colname.unit).alias(Colname.unit),
            min(Colname.observation_time).alias(Colname.charge_time),
            flatten(collect_set(Colname.qualities)).alias(Colname.qualities),
        )
        .orderBy(Colname.charge_time)
        .select(
            col(Colname.grid_area),
            col(Colname.energy_supplier_id),
            col(Colname.total_quantity),
            col(Colname.unit),
            col(Colname.qualities),
            col(Colname.charge_time),
            lit(ChargeResolution.MONTH.value).alias(Colname.charge_resolution),
            lit(None).alias(Colname.metering_point_type),
            lit(None).alias(Colname.settlement_method),
            col(Colname.charge_price),
            col(Colname.total_amount),
            col(Colname.charge_tax),
            col(Colname.charge_id),
            col(Colname.charge_type),
            col(Colname.charge_owner),
        )
    )

    return agg_df

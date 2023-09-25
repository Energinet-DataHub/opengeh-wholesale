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
import pyspark.sql.functions as F
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
from datetime import datetime


def execute(
    calculation_input_reader: CalculationInputReader,
    wholesale_calculation_result_writer: WholesaleCalculationResultWriter,
    metering_points_periods_df: DataFrame,  # TODO: use enriched_time_series
    time_series_point_df: DataFrame,  # TODO: use enriched_time_series
    period_start_datetime: datetime,
) -> None:
    # Get input data
    metering_points_periods_df = _get_production_and_consumption_metering_points(
        metering_points_periods_df
    )
    charge_master_data = calculation_input_reader.read_charge_master_data_periods()
    charge_links = calculation_input_reader.read_charge_links_periods()
    charge_prices = calculation_input_reader.read_charge_price_points()

    # Calculate and write to storage
    _calculate_tariff_charges(
        wholesale_calculation_result_writer,
        metering_points_periods_df,
        time_series_point_df,
        charge_master_data,
        charge_links,
        charge_prices,
        period_start_datetime,
    )


def _calculate_tariff_charges(
    wholesale_calculation_result_writer: WholesaleCalculationResultWriter,
    metering_points_periods_df: DataFrame,
    time_series_point_df: DataFrame,
    charge_master_data: DataFrame,
    charge_links: DataFrame,
    charge_prices: DataFrame,
    period_start_datetime: datetime,
) -> None:
    tariffs_hourly = init.get_tariff_charges(
        metering_points_periods_df,
        time_series_point_df,
        charge_master_data,
        charge_links,
        charge_prices,
        ChargeResolution.HOUR,
    )

    hourly_tariff_per_ga_co_es = calculate_tariff_price_per_ga_co_es(tariffs_hourly)
    wholesale_calculation_result_writer.write(hourly_tariff_per_ga_co_es)

    monthly_tariff_per_ga_co_es = sum_within_month(
        hourly_tariff_per_ga_co_es, period_start_datetime
    )
    wholesale_calculation_result_writer.write(monthly_tariff_per_ga_co_es)


def _get_production_and_consumption_metering_points(
    metering_points_periods_df: DataFrame,
) -> DataFrame:
    return metering_points_periods_df.filter(
        (F.col(Colname.metering_point_type) == MeteringPointType.CONSUMPTION.value)
        | (F.col(Colname.metering_point_type) == MeteringPointType.PRODUCTION.value)
    )


def sum_within_month(df: DataFrame, period_start_datetime: datetime) -> DataFrame:
    agg_df = (
        df.groupBy(
            Colname.energy_supplier_id,
            Colname.grid_area,
            Colname.charge_key,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
        )
        .agg(
            F.sum(Colname.total_amount).alias(Colname.total_amount),
            F.sum(Colname.total_quantity).alias(Colname.total_quantity),
            F.sum(Colname.charge_price).alias(Colname.charge_price),
            # charge_tax is the same for all tariffs in a given month
            F.first(Colname.charge_tax).alias(Colname.charge_tax),
            # tariff unit is the same for all tariffs in a given month (kWh)
            F.first(Colname.unit).alias(Colname.unit),
            F.flatten(F.collect_set(Colname.qualities)).alias(Colname.qualities),
        )
        .select(
            F.col(Colname.grid_area),
            F.col(Colname.energy_supplier_id),
            F.col(Colname.total_quantity),
            F.col(Colname.unit),
            F.col(Colname.qualities),
            F.lit(period_start_datetime).alias(Colname.charge_time),
            F.lit(ChargeResolution.MONTH.value).alias(Colname.charge_resolution),
            F.lit(None).alias(Colname.metering_point_type),
            F.lit(None).alias(Colname.settlement_method),
            F.col(Colname.charge_price),
            F.col(Colname.total_amount),
            F.col(Colname.charge_tax),
            F.col(Colname.charge_id),
            F.col(Colname.charge_type),
            F.col(Colname.charge_owner),
        )
    )

    return agg_df

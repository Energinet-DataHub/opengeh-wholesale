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
from pyspark.sql.functions import col, when
import package.calculation.wholesale.wholesale_initializer as init
from package.calculation.wholesale.tariff_calculators import calculate_tariff_price_per_ga_co_es
from package.codelists import ChargeResolution, MeteringPointType
from package.constants import Colname
from package.calculation_input import CalculationInputReader


def execute(
    calculation_input_reader: CalculationInputReader,
    metering_points_periods_df: DataFrame,  # TODO: use enriched_time_series
    time_series_point_df: DataFrame,  # TODO: use enriched_time_series
) -> None:

    # Get input data
    metering_points_periods_df = _get_production_and_consumption_metering_points(metering_points_periods_df)
    charge_master_data = calculation_input_reader.read_charge_master_data_periods()
    charge_links = calculation_input_reader.read_charge_links_periods()
    charge_prices = calculation_input_reader.read_charge_price_points()

    # Calculate and write to storage
    _calculate_tariff_charges(metering_points_periods_df,
                              time_series_point_df,
                              charge_master_data,
                              charge_links,
                              charge_prices)


def _calculate_tariff_charges(
    metering_points_periods_df: DataFrame,
    time_series_point_df: DataFrame,
    charge_master_data: DataFrame,
    charge_links: DataFrame,
    charge_prices: DataFrame
) -> None:

    tariffs_hourly = init.get_tariff_charges(
        metering_points_periods_df,
        time_series_point_df,
        charge_master_data,
        charge_links,
        charge_prices,
        ChargeResolution.HOUR
    )

    hourly_tariff_per_ga_co_es = calculate_tariff_price_per_ga_co_es(tariffs_hourly)
    hourly_tariff_per_ga_co_es.printSchema()  # TODO JMG: remove and write to storage instead


def _get_production_and_consumption_metering_points(metering_points_periods_df: DataFrame) -> DataFrame:
    return metering_points_periods_df.filter(
        (col(Colname.metering_point_type) == MeteringPointType.CONSUMPTION.value)
        | (col(Colname.metering_point_type) == MeteringPointType.PRODUCTION.value)
    )

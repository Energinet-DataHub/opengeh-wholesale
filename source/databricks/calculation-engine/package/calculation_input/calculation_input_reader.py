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

from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.functions import concat_ws, col, when, lit
from package.codelists import (
    InputMeteringPointType,
    InputSettlementMethod,
    MeteringPointType,
    SettlementMethod,
)
from package.constants import Colname
from package.infrastructure import paths


class CalculationInputReader:
    def __init__(
        self,
        spark: SparkSession,
    ) -> None:
        self.__spark = spark

    def read_metering_point_periods(self) -> DataFrame:
        df = self.__spark.read.table(f"{paths.INPUT_DATABASE_NAME}.{paths.METERING_POINT_PERIODS_TABLE_NAME}")
        df = _fix_settlement_method(df)
        df = _fix_metering_point_type(df)
        return df

    def read_time_series_points(self) -> DataFrame:
        return self.__spark.read.table(f"{paths.INPUT_DATABASE_NAME}.{paths.TIME_SERIES_POINTS_TABLE_NAME}")

    def read_charge_links_periods(self) -> DataFrame:
        df = self.__spark.read.table(f"{paths.INPUT_DATABASE_NAME}.{paths.CHARGE_LINK_PERIODS_TABLE_NAME}")
        df = _add_charge_key_column(df)
        return df

    def read_charge_master_data_periods(self) -> DataFrame:
        df = self.__spark.read.table(f"{paths.INPUT_DATABASE_NAME}.{paths.CHARGE_MASTER_DATA_PERIODS_TABLE_NAME}")
        df = _add_charge_key_column(df)
        return df

    def read_charge_price_points(self) -> DataFrame:
        df = self.__spark.read.table(f"{paths.INPUT_DATABASE_NAME}.{paths.CHARGE_PRICE_POINTS_TABLE_NAME}")
        df = _add_charge_key_column(df)
        return df


def _fix_metering_point_type(df: DataFrame) -> DataFrame:
    return df.withColumn(
        Colname.metering_point_type,
        when(col(Colname.metering_point_type) == InputMeteringPointType.CONSUMPTION.value, lit(MeteringPointType.CONSUMPTION.value))
        .when(col(Colname.metering_point_type) == InputMeteringPointType.PRODUCTION.value, lit(MeteringPointType.PRODUCTION.value))
        .when(col(Colname.metering_point_type) == InputMeteringPointType.EXCHANGE.value, lit(MeteringPointType.EXCHANGE.value))
        .when(col(Colname.metering_point_type) == InputMeteringPointType.VE_PRODUCTION.value, lit(MeteringPointType.VE_PRODUCTION.value))
        .when(col(Colname.metering_point_type) == InputMeteringPointType.NET_PRODUCTION.value, lit(MeteringPointType.NET_PRODUCTION.value))
        .when(col(Colname.metering_point_type) == InputMeteringPointType.SUPPLY_TO_GRID.value, lit(MeteringPointType.SUPPLY_TO_GRID.value))
        .when(col(Colname.metering_point_type) == InputMeteringPointType.CONSUMPTION_FROM_GRID.value,
              lit(MeteringPointType.CONSUMPTION_FROM_GRID.value))
        .when(col(Colname.metering_point_type) == InputMeteringPointType.WHOLESALE_SERVICES_INFORMATION.value,
              lit(MeteringPointType.WHOLESALE_SERVICES_INFORMATION.value))
        .when(col(Colname.metering_point_type) == InputMeteringPointType.OWN_PRODUCTION.value, lit(MeteringPointType.OWN_PRODUCTION.value))
        .when(col(Colname.metering_point_type) == InputMeteringPointType.NET_FROM_GRID.value, lit(MeteringPointType.NET_FROM_GRID.value))
        .when(col(Colname.metering_point_type) == InputMeteringPointType.NET_TO_GRID.value, lit(MeteringPointType.NET_TO_GRID.value))
        .when(col(Colname.metering_point_type) == InputMeteringPointType.TOTAL_CONSUMPTION.value, lit(MeteringPointType.TOTAL_CONSUMPTION.value))
        .when(col(Colname.metering_point_type) == InputMeteringPointType.ELECTRICAL_HEATING.value, lit(MeteringPointType.ELECTRICAL_HEATING.value))
        .when(col(Colname.metering_point_type) == InputMeteringPointType.NET_CONSUMPTION.value, lit(MeteringPointType.NET_CONSUMPTION.value))
        .when(col(Colname.metering_point_type) == InputMeteringPointType.EFFECT_SETTLEMENT.value, lit(MeteringPointType.EFFECT_SETTLEMENT.value))
    )


def _fix_settlement_method(df: DataFrame) -> DataFrame:
    return df.withColumn(
        Colname.settlement_method,
        when(col(Colname.settlement_method) == InputSettlementMethod.FLEX.value, lit(SettlementMethod.FLEX.value))
        .when(col(Colname.settlement_method) == InputSettlementMethod.NON_PROFILED.value, lit(SettlementMethod.NON_PROFILED.value))
    )


def _add_charge_key_column(charge_df: DataFrame) -> DataFrame:
    return charge_df.withColumn(Colname.charge_key, concat_ws("-", col(Colname.charge_id), col(Colname.charge_owner), col(Colname.charge_type)))

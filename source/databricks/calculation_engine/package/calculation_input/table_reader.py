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

from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.functions import concat_ws, col, when, lit

from package.codelists import (
    InputMeteringPointType,
    InputSettlementMethod,
    MeteringPointType,
    SettlementMethod,
)
from package.common import assert_schema
from package.constants import Colname
from package.infrastructure import paths
from .schemas import (
    charge_link_periods_schema,
    charge_master_data_periods_schema,
    charge_price_points_schema,
    metering_point_period_schema,
    time_series_point_schema,
)


class TableReader:
    def __init__(
        self,
        spark: SparkSession,
        calculation_input_path: str,
        time_series_points_table_name: str | None = None,
        metering_point_periods_table_name: str | None = None,
    ) -> None:
        self._spark = spark
        self._calculation_input_path = calculation_input_path
        self._time_series_points_table_name = (
            time_series_points_table_name or paths.TIME_SERIES_POINTS_TABLE_NAME
        )
        self._metering_point_periods_table_name = (
            metering_point_periods_table_name or paths.METERING_POINT_PERIODS_TABLE_NAME
        )

    def read_metering_point_periods(
        self,
        period_start_datetime: datetime,
        period_end_datetime: datetime,
        calculation_grid_areas: list[str],
    ) -> DataFrame:
        path = (
            f"{self._calculation_input_path}/{self._metering_point_periods_table_name}"
        )
        df = (
            self._spark.read.format("delta")
            .load(path)
            .where(
                col(Colname.grid_area).isin(calculation_grid_areas)
                | col(Colname.from_grid_area).isin(calculation_grid_areas)
                | col(Colname.to_grid_area).isin(calculation_grid_areas)
            )
            .where(col(Colname.from_date) < period_end_datetime)
            .where(
                col(Colname.to_date).isNull()
                | (col(Colname.to_date) > period_start_datetime)
            )
        )

        assert_schema(df.schema, metering_point_period_schema)

        df = self._fix_settlement_method(df)
        df = self._fix_metering_point_type(df)
        return df

    def read_time_series_points(
        self, period_start_datetime: datetime, period_end_datetime: datetime
    ) -> DataFrame:
        path = f"{self._calculation_input_path}/{self._time_series_points_table_name}"

        if "_partitioned_by_year_and_month" in path:
            # TEMPORARY: This is for experimenting with a table that is partitioned by year and month.
            # TODO: Cleanup when/if the table is time_series_points  table is partitioned by year and month.

            if period_start_datetime.year != period_end_datetime.year:
                raise ValueError(
                    "The period start and end datetimes must be within the same year"
                )
            if period_start_datetime.month != period_end_datetime.month:
                raise ValueError(
                    "The period start and end datetimes must be within the same month"
                )
            year = period_start_datetime.year
            month = period_start_datetime.month
            df = (
                self._spark.read.format("delta")
                .load(path)
                .where(col("observation_year") == year)
                .where(col("observation_month") == month)
                .where(col(Colname.observation_time) >= period_start_datetime)
                .where(col(Colname.observation_time) < period_end_datetime)
            )
        else:
            df = (
                self._spark.read.format("delta")
                .load(path)
                .where(col(Colname.observation_time) >= period_start_datetime)
                .where(col(Colname.observation_time) < period_end_datetime)
            )

        assert_schema(df.schema, time_series_point_schema)

        return df

    def read_charge_links_periods(self) -> DataFrame:
        path = f"{self._calculation_input_path}/{paths.CHARGE_LINK_PERIODS_TABLE_NAME}"
        df = self._spark.read.format("delta").load(path)

        assert_schema(df.schema, charge_link_periods_schema)

        df = self._add_charge_key_column(df)
        return df

    def read_charge_master_data_periods(self) -> DataFrame:
        path = f"{self._calculation_input_path}/{paths.CHARGE_MASTER_DATA_PERIODS_TABLE_NAME}"
        df = self._spark.read.format("delta").load(path)

        assert_schema(df.schema, charge_master_data_periods_schema)

        df = self._add_charge_key_column(df)
        return df

    def read_charge_price_points(self) -> DataFrame:
        path = f"{self._calculation_input_path}/{paths.CHARGE_PRICE_POINTS_TABLE_NAME}"
        df = self._spark.read.format("delta").load(path)

        assert_schema(df.schema, charge_price_points_schema)

        df = self._add_charge_key_column(df)
        return df

    def _add_charge_key_column(self, charge_df: DataFrame) -> DataFrame:
        return charge_df.withColumn(
            Colname.charge_key,
            concat_ws(
                "-",
                col(Colname.charge_code),
                col(Colname.charge_owner),
                col(Colname.charge_type),
            ),
        )

    def _fix_metering_point_type(self, df: DataFrame) -> DataFrame:
        return df.withColumn(
            Colname.metering_point_type,
            when(
                col(Colname.metering_point_type)
                == InputMeteringPointType.CONSUMPTION.value,
                lit(MeteringPointType.CONSUMPTION.value),
            )
            .when(
                col(Colname.metering_point_type)
                == InputMeteringPointType.PRODUCTION.value,
                lit(MeteringPointType.PRODUCTION.value),
            )
            .when(
                col(Colname.metering_point_type)
                == InputMeteringPointType.EXCHANGE.value,
                lit(MeteringPointType.EXCHANGE.value),
            )
            .when(
                col(Colname.metering_point_type)
                == InputMeteringPointType.VE_PRODUCTION.value,
                lit(MeteringPointType.VE_PRODUCTION.value),
            )
            .when(
                col(Colname.metering_point_type)
                == InputMeteringPointType.NET_PRODUCTION.value,
                lit(MeteringPointType.NET_PRODUCTION.value),
            )
            .when(
                col(Colname.metering_point_type)
                == InputMeteringPointType.SUPPLY_TO_GRID.value,
                lit(MeteringPointType.SUPPLY_TO_GRID.value),
            )
            .when(
                col(Colname.metering_point_type)
                == InputMeteringPointType.CONSUMPTION_FROM_GRID.value,
                lit(MeteringPointType.CONSUMPTION_FROM_GRID.value),
            )
            .when(
                col(Colname.metering_point_type)
                == InputMeteringPointType.WHOLESALE_SERVICES_INFORMATION.value,
                lit(MeteringPointType.WHOLESALE_SERVICES_INFORMATION.value),
            )
            .when(
                col(Colname.metering_point_type)
                == InputMeteringPointType.OWN_PRODUCTION.value,
                lit(MeteringPointType.OWN_PRODUCTION.value),
            )
            .when(
                col(Colname.metering_point_type)
                == InputMeteringPointType.NET_FROM_GRID.value,
                lit(MeteringPointType.NET_FROM_GRID.value),
            )
            .when(
                col(Colname.metering_point_type)
                == InputMeteringPointType.NET_TO_GRID.value,
                lit(MeteringPointType.NET_TO_GRID.value),
            )
            .when(
                col(Colname.metering_point_type)
                == InputMeteringPointType.TOTAL_CONSUMPTION.value,
                lit(MeteringPointType.TOTAL_CONSUMPTION.value),
            )
            .when(
                col(Colname.metering_point_type)
                == InputMeteringPointType.ELECTRICAL_HEATING.value,
                lit(MeteringPointType.ELECTRICAL_HEATING.value),
            )
            .when(
                col(Colname.metering_point_type)
                == InputMeteringPointType.NET_CONSUMPTION.value,
                lit(MeteringPointType.NET_CONSUMPTION.value),
            )
            .when(
                col(Colname.metering_point_type)
                == InputMeteringPointType.EFFECT_SETTLEMENT.value,
                lit(MeteringPointType.EFFECT_SETTLEMENT.value),
            )
            .otherwise(lit("Unknown type")),
            # The otherwise is to avoid changing the nullability of the column.
        )

    @staticmethod
    def _fix_settlement_method(df: DataFrame) -> DataFrame:
        return df.withColumn(
            Colname.settlement_method,
            when(
                col(Colname.settlement_method) == InputSettlementMethod.FLEX.value,
                lit(SettlementMethod.FLEX.value),
            ).when(
                col(Colname.settlement_method)
                == InputSettlementMethod.NON_PROFILED.value,
                lit(SettlementMethod.NON_PROFILED.value),
            ),
        )

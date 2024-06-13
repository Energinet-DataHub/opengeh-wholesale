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

from package.common import assert_schema
from package.infrastructure.paths import InputDatabase, BasisDataDatabase
from .schemas import (
    charge_link_periods_schema,
    charge_price_information_periods_schema,
    charge_price_points_schema,
    metering_point_period_schema,
    time_series_point_schema,
    grid_loss_metering_points_schema,
)
from ..basis_data.schemas import calculations_schema


class TableReader:
    def __init__(
        self,
        spark: SparkSession,
        calculation_input_path: str,
        time_series_points_table_name: str | None = None,
        metering_point_periods_table_name: str | None = None,
        grid_loss_metering_points_table_name: str | None = None,
    ) -> None:
        self._spark = spark
        self._calculation_input_path = calculation_input_path
        self._time_series_points_table_name = (
            time_series_points_table_name or InputDatabase.TIME_SERIES_POINTS_TABLE_NAME
        )
        self._metering_point_periods_table_name = (
            metering_point_periods_table_name
            or InputDatabase.METERING_POINT_PERIODS_TABLE_NAME
        )
        self._grid_loss_metering_points_table_name = (
            grid_loss_metering_points_table_name
            or InputDatabase.GRID_LOSS_METERING_POINTS_TABLE_NAME
        )

    def read_metering_point_periods(
        self,
    ) -> DataFrame:
        path = (
            f"{self._calculation_input_path}/{self._metering_point_periods_table_name}"
        )
        df = self._spark.read.format("delta").load(path)

        assert_schema(df.schema, metering_point_period_schema)

        return df

    def read_time_series_points(self) -> DataFrame:
        path = f"{self._calculation_input_path}/{self._time_series_points_table_name}"

        df = self._spark.read.format("delta").load(path)

        # Data migration have added this extra column. But for now we prefer to ignore it rather than
        # update the expected schema and use it.
        df = df.drop("partition_date_col")

        assert_schema(df.schema, time_series_point_schema)

        return df

    def read_charge_link_periods(self) -> DataFrame:
        path = f"{self._calculation_input_path}/{InputDatabase.CHARGE_LINK_PERIODS_TABLE_NAME}"
        df = self._spark.read.format("delta").load(path)

        assert_schema(df.schema, charge_link_periods_schema)

        return df

    def read_charge_price_information_periods(self) -> DataFrame:
        path = f"{self._calculation_input_path}/{InputDatabase.CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME}"
        df = self._spark.read.format("delta").load(path)

        assert_schema(df.schema, charge_price_information_periods_schema)

        return df

    def read_charge_price_points(
        self,
    ) -> DataFrame:
        path = f"{self._calculation_input_path}/{InputDatabase.CHARGE_PRICE_POINTS_TABLE_NAME}"
        df = self._spark.read.format("delta").load(path)

        assert_schema(df.schema, charge_price_points_schema)

        return df

    def read_grid_loss_metering_points(self) -> DataFrame:
        path = f"{self._calculation_input_path}/{self._grid_loss_metering_points_table_name}"
        df = self._spark.read.format("delta").load(path)

        assert_schema(df.schema, grid_loss_metering_points_schema)

        return df

    def read_calculations(self) -> DataFrame:
        table_name = f"{BasisDataDatabase.DATABASE_NAME}.{BasisDataDatabase.CALCULATIONS_TABLE_NAME}"
        df = self._spark.read.format("delta").table(table_name)

        assert_schema(df.schema, calculations_schema)

        return df

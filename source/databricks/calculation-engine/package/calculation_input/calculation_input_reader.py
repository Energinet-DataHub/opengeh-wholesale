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
from pyspark.sql.functions import concat_ws, col
from package.constants import Colname
from package.infrastructure import paths


class CalculationInputReader:
    def __init__(
        self,
        spark: SparkSession,
    ) -> None:
        self.__spark = spark

    def read_metering_point_periods(self) -> DataFrame:
        return self.__spark.read.table(f"{paths.INPUT_DATABASE_NAME}.{paths.METERING_POINT_PERIODS_TABLE_NAME}")

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


def _add_charge_key_column(charge_df: DataFrame) -> DataFrame:
    return charge_df.withColumn(Colname.charge_key, concat_ws("-", col(Colname.charge_id), col(Colname.charge_owner), col(Colname.charge_type)))

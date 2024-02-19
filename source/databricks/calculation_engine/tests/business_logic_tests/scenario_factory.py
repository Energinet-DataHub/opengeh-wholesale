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
import os
import uuid
from pathlib import Path
from unittest.mock import Mock

import pandas as pd
import yaml
from azure.identity import ClientSecretCredential
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql import functions as f
from pyspark.sql.functions import lit, col
from pyspark.sql.types import (
    StructType,
    StringType,
    TimestampType,
    StructField,
    BooleanType,
    DecimalType,
    ArrayType,
)

from business_logic_tests.features.csv_file_loader import read_files_in_parallel
from package.calculation import PreparedDataReader
from package.calculation.CalculationResults import CalculationResultsContainer
from package.calculation.calculation import execute_calculation
from package.calculation.calculator_args import CalculatorArgs
from package.calculation_input.schemas import (
    metering_point_period_schema,
    time_series_point_schema,
    grid_loss_metering_points_schema,
    charge_link_periods_schema,
    charge_master_data_periods_schema,
    charge_price_points_schema,
)
from package.codelists import CalculationType
from package.constants import Colname


class ScenarioFactory:

    table_reader: Mock
    calculation_args: CalculatorArgs
    test_path: str

    def __init__(self, spark: SparkSession, file_path: Path):
        self.spark = spark
        self.table_reader = Mock()
        self.test_path = os.path.dirname(file_path) + "/test_data/"

        file_paths = [
            "os.path.dirname(file_path) + " / test_data / " + charge_link_periods.csv",
            "charge_price_points.csv",
        ]
        frames = read_files_in_parallel(self.spark, file_paths)

        # Show the dataframes
        for frame in frames:
            frame.show()

        # self._load_test_data()

    def _load_test_data(self) -> None:
        self.calculation_args = self._load_calculation_args()
        self._load_metering_point_periods()
        self._load_time_series_points()
        self._load_grid_loss_metering_points()
        self._load_charge_link_periods()
        self._load_charge_master_data_periods()
        self._load_charge_price_points()

    def _load_metering_point_periods(self) -> None:
        df = self._parse_csv_to_dataframe(
            "metering_point_periods.csv", metering_point_period_schema
        )
        self.table_reader.read_metering_point_periods.return_value = df

    def _load_time_series_points(self) -> None:
        df = self._parse_csv_to_dataframe(
            "time_series_points.csv", time_series_point_schema
        )
        self.table_reader.read_time_series_points.return_value = df

    def _load_grid_loss_metering_points(self) -> None:
        df = self._parse_csv_to_dataframe(
            "grid_loss_metering_points.csv", grid_loss_metering_points_schema
        )
        self.table_reader.read_grid_loss_metering_points.return_value = df

    def _load_charge_link_periods(self) -> None:
        df = self._parse_csv_to_dataframe(
            "charge_link_periods.csv", charge_link_periods_schema
        )
        self.table_reader.read_charge_links_periods.return_value = df

    def _load_charge_master_data_periods(self) -> None:
        df = self._parse_csv_to_dataframe(
            "charge_master_data_periods.csv", charge_master_data_periods_schema
        )
        self.table_reader.read_charge_master_data_periods.return_value = df

    def _load_charge_price_points(self) -> None:
        df = self._parse_csv_to_dataframe(
            "charge_price_points.csv", charge_price_points_schema
        )
        self.table_reader.read_charge_price_points.return_value = df

    def _parse_csv_to_dataframe(self, file_name: str, schema: str) -> DataFrame | None:
        # if file exists
        # if not os.path.exists(self.test_path + file_name):
        #   return None

        df = self.spark.read.csv(
            self.test_path + file_name, header=True, schema=schema, sep=";"
        )
        return self.spark.createDataFrame(df.rdd, schema)

    def _load_calculation_args(self) -> CalculatorArgs:
        with open(self.test_path + "calculation_arguments.yml", "r") as file:
            calculation_args = yaml.safe_load(file)

        # Convert the row to a CalculatorArgs instance
        return CalculatorArgs(
            data_storage_account_name="foo",
            data_storage_account_credentials=ClientSecretCredential(
                "foo", "foo", "foo"
            ),
            wholesale_container_path="foo",
            calculation_input_path="foo",
            time_series_points_table_name=None,
            metering_point_periods_table_name=None,
            grid_loss_metering_points_table_name=None,  # Are they needed? After Bjarke's PR
            calculation_id=str(uuid.uuid4()),
            calculation_grid_areas=calculation_args[0]["grid_areas"],
            calculation_period_start_datetime=pd.to_datetime(
                calculation_args[0]["period_start"], format="%Y-%m-%d %H:%M:%S"
            ),
            calculation_period_end_datetime=pd.to_datetime(
                calculation_args[0]["period_end"], format="%Y-%m-%d %H:%M:%S"
            ),
            calculation_type=CalculationType(calculation_args[0]["calculation_type"]),
            calculation_execution_time_start=pd.to_datetime(
                calculation_args[0]["calculation_execution_time_start"],
                format="%Y-%m-%d %H:%M:%S",
            ),
            time_zone="Europe/Copenhagen",  # TODO const?
        )

    def execute_scenario(self) -> CalculationResultsContainer:
        return execute_calculation(
            self.calculation_args, PreparedDataReader(self.table_reader)
        )

    def get_expected_result(self) -> DataFrame:
        df = self.spark.read.csv(
            self.test_path + "results.csv",
            header=True,
            sep=";",
        )
        if df.rdd.isEmpty():
            raise ValueError("The DataFrame is empty, cannot create RDD")

        df = df.withColumn("calculation_id", lit(self.calculation_args.calculation_id))
        df = df.withColumn(
            Colname.calculation_execution_time_start,
            lit(self.calculation_args.calculation_execution_time_start).cast(
                TimestampType()
            ),
        )
        df = df.withColumn("calculation_result_id", lit(""))
        df = df.withColumn("quantity_unit", lit("kWh"))  # TODO AJW
        df = df.withColumn("quantity", col("quantity").cast(DecimalType(28, 3)))
        df = df.withColumn("price", col("price").cast(DecimalType(28, 3)))
        df = df.withColumn("amount", col("amount").cast(DecimalType(38, 6)))
        df = df.withColumn("time", col("time").cast(TimestampType()))
        df = df.withColumn("is_tax", col("is_tax").cast(BooleanType()))
        df = df.withColumn("settlement_method", lit("flex"))  # TODO AJW
        df = df.withColumn(
            "quantity_qualities",
            f.split(f.col("quantity_qualities"), ",").cast(ArrayType(StringType())),
        )

        return self.spark.createDataFrame(df.rdd, schema)


schema = StructType(
    [
        StructField("calculation_id", StringType(), False),
        StructField("calculation_type", StringType(), False),
        StructField("calculation_execution_time_start", TimestampType(), False),
        StructField("calculation_result_id", StringType(), True),
        StructField("grid_area", StringType(), False),
        StructField("energy_supplier_id", StringType(), True),
        StructField("quantity", DecimalType(28, 3), True),
        StructField("quantity_unit", StringType(), False),
        StructField("quantity_qualities", ArrayType(StringType()), False),
        StructField("time", TimestampType(), False),
        StructField("resolution", StringType(), False),
        StructField("metering_point_type", StringType(), False),
        StructField("settlement_method", StringType(), True),
        StructField("price", DecimalType(18, 6), False),
        StructField("amount", DecimalType(38, 6), True),
        StructField("is_tax", BooleanType(), False),
        StructField("charge_code", StringType(), False),
        StructField("charge_type", StringType(), False),
        StructField("charge_owner_id", StringType(), False),
        StructField("amount_type", StringType(), False),
    ]
)

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
from ast import literal_eval
from datetime import datetime

from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.functions import col, udf
from pyspark.sql.types import (
    StringType,
    TimestampType,
    DecimalType,
    ArrayType,
    StructType,
    StructField,
)

from package.calculation.CalculationResults import (
    EnergyResultsContainer,
    CalculationResultsContainer,
)
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.energy.energy_results import (
    EnergyResults,
    energy_results_schema,
)
from package.constants import Colname

schema = StructType(
    [
        StructField(Colname.calculation_id, StringType(), False),
        StructField(Colname.calculation_type, StringType(), False),
        StructField(Colname.calculation_execution_time_start, TimestampType(), False),
        StructField("calculation_result_id", StringType(), True),
        StructField("grid_area", StringType(), False),
        StructField(Colname.energy_supplier_id, StringType(), True),
        StructField(Colname.quantity, DecimalType(28, 3), True),
        StructField("quantity_unit", StringType(), False),
        StructField("quantity_qualities", ArrayType(StringType()), False),
        StructField("time", TimestampType(), False),
        StructField(Colname.resolution, StringType(), False),
        StructField("metering_point_type", StringType(), False),
        StructField(Colname.settlement_method, StringType(), True),
        StructField(Colname.charge_code, StringType(), False),
        StructField(Colname.charge_type, StringType(), False),
        StructField(Colname.charge_owner, StringType(), False),
        StructField("amount_type", StringType(), False),
    ]
)


time_window_schema = StructType(
    [
        StructField("start", TimestampType()),
        StructField("end", TimestampType()),
    ]
)


def parse_time_window(time_window_str):
    time_window_str = time_window_str.replace("{", "").replace("}", "")
    start_str, end_str = time_window_str.split(",")
    start = datetime.strptime(start_str, "%Y-%m-%d %H:%M:%S")
    end = datetime.strptime(end_str, "%Y-%m-%d %H:%M:%S")
    return start, end


def parse_qualities_string(qualities_str):
    return literal_eval(qualities_str)


def get_result(
    spark: SparkSession, calculation_args: CalculatorArgs, df: DataFrame
) -> CalculationResultsContainer:

    parse_time_window_udf = udf(
        parse_time_window,
        StructType(
            [StructField("start", TimestampType()), StructField("end", TimestampType())]
        ),
    )

    df = df.withColumn("time_window", parse_time_window_udf(df["time_window"]))
    df = df.withColumn(
        Colname.sum_quantity, col(Colname.sum_quantity).cast(DecimalType(18, 6))
    )

    parse_qualities_string_udf = udf(parse_qualities_string, ArrayType(StringType()))
    df = df.withColumn("quantity", parse_qualities_string_udf(df["quantity"]))
    df = df.withColumnRenamed("quantity", "qualities")

    results = CalculationResultsContainer()
    results.energy_results = EnergyResultsContainer()
    results.energy_results.flex_consumption_per_ga_and_es = EnergyResults(
        spark.createDataFrame(df.rdd, energy_results_schema),
    )

    return results

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
from pyspark.sql import functions as f, DataFrame, SparkSession
from pyspark.sql.functions import col
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


def get_result(spark: SparkSession, df: DataFrame) -> CalculationResultsContainer:
    df = df.withColumn(
        Colname.time_window, col(Colname.time_window).cast(TimestampType())
    )
    df = df.withColumn(
        Colname.sum_quantity, col(Colname.sum_quantity).cast(DecimalType(28, 3))
    )
    df = df.withColumn(
        "quantity_qualities",
        f.split(
            f.regexp_replace(
                f.regexp_replace(f.col("quantity_qualities"), r"[\[\]']", ""), " ", ""
            ),
            ",",
        ).cast(ArrayType(StringType())),
    )

    results = CalculationResultsContainer()
    results.energy_results = EnergyResultsContainer()
    results.energy_results.flex_consumption_per_ga_and_es = EnergyResults(
        spark.createDataFrame(df.rdd, energy_results_schema),
    )
    return results

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
from pyspark.sql.functions import lit, col
from pyspark.sql.types import (
    StringType,
    TimestampType,
    BooleanType,
    DecimalType,
    ArrayType,
    StructType,
    StructField,
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
        StructField("price", DecimalType(18, 6), False),
        StructField("amount", DecimalType(38, 6), True),
        StructField("is_tax", BooleanType(), False),
        StructField(Colname.charge_code, StringType(), False),
        StructField(Colname.charge_type, StringType(), False),
        StructField(Colname.charge_owner, StringType(), False),
        StructField("amount_type", StringType(), False),
    ]
)


def get_expected(*args) -> DataFrame:  # type: ignore
    spark: SparkSession = args[0]
    df: DataFrame = args[1]
    calculation_args = args[2]

    df = df.withColumn(Colname.calculation_id, lit(calculation_args.calculation_id))
    df = df.withColumn(
        Colname.calculation_execution_time_start,
        lit(calculation_args.calculation_execution_time_start).cast(TimestampType()),
    )
    df = df.withColumn("calculation_result_id", lit(""))
    df = df.withColumn("quantity_unit", lit("kWh"))  # TODO AJW
    df = df.withColumn("quantity", col("quantity").cast(DecimalType(28, 3)))
    df = df.withColumn("price", col("price").cast(DecimalType(28, 3)))
    df = df.withColumn("amount", col("amount").cast(DecimalType(38, 6)))
    df = df.withColumn("time", col("time").cast(TimestampType()))
    df = df.withColumn("is_tax", col("is_tax").cast(BooleanType()))
    df = df.withColumn(Colname.settlement_method, lit("flex"))  # TODO AJW

    df = df.withColumn(
        "quantity_qualities",
        f.split(
            f.regexp_replace(
                f.regexp_replace(f.col("quantity_qualities"), r"[\[\]']", ""), " ", ""
            ),
            ",",
        ).cast(ArrayType(StringType())),
    )

    return spark.createDataFrame(df.rdd, schema)

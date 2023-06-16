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
"""
By having a conftest.py in this directory, we are able to add all packages
defined in the geh_stream directory in our tests.
"""

import os
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.types import StringType, StructType, StructField, TimestampType
from package.constants import Colname
from pyspark.sql.functions import col, when


def get_grid_loss_responsible() -> DataFrame:
    script_dir = os.path.dirname(os.path.normpath(__file__))

    file_path = os.path.join(script_dir, 'GridLossResponsible.csv')

    schema = StructType([
        StructField("METERING_POINT_ID", StringType(), nullable=False),
        StructField("GRID_AREA", StringType(), nullable=False),
        StructField("VALID_FROM", TimestampType(), nullable=False),
        StructField("VALID_TO", TimestampType(), nullable=True),
        StructField("TYPE_OF_MP", StringType(), nullable=False),
        StructField("BALANCE_SUPPLIER_ID", StringType(), nullable=False)
    ])

    spark = SparkSession.builder.getOrCreate()
    grid_loss_responsible_df = spark.read.option("header", True).csv(file_path, schema=schema)

    grid_loss_responsible_df = grid_loss_responsible_df.select(
        col("METERING_POINT_ID").alias(Colname.metering_point_id),
        col("GRID_AREA").alias(Colname.grid_area),
        col("VALID_FROM").alias(Colname.from_date),
        col("VALID_TO").alias(Colname.to_date),
        col("TYPE_OF_MP").alias(Colname.metering_point_type),
        col("BALANCE_SUPPLIER_ID").alias(Colname.energy_supplier_id),
    )
    grid_loss_responsible_df = grid_loss_responsible_df.withColumn(
        Colname.is_positive_grid_loss_responsible, when(col(Colname.metering_point_type) == "consumption", True).otherwise(False))
    grid_loss_responsible_df = grid_loss_responsible_df.withColumn(
        Colname.is_negative_grid_loss_responsible, when(col(Colname.metering_point_type) == "production", True).otherwise(False))

    return grid_loss_responsible_df

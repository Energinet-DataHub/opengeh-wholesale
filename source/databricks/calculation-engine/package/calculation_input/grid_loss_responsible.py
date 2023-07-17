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
from datetime import datetime


def get_grid_loss_responsible(grid_areas: list[str]) -> DataFrame:
    # script_dir = os.path.dirname(os.path.normpath(__file__))

    # file_path = os.path.join(script_dir, 'GridLossResponsible.csv')

    schema = StructType([
        StructField(Colname.metering_point_id, StringType(), nullable=False),
        StructField(Colname.grid_area, StringType(), nullable=False),
        StructField(Colname.from_date, TimestampType(), nullable=False),
        StructField(Colname.to_date, TimestampType(), nullable=True),
        StructField(Colname.metering_point_type, StringType(), nullable=False),
        StructField(Colname.energy_supplier_id, StringType(), nullable=False)
    ])

    spark = SparkSession.builder.getOrCreate()
    # grid_loss_responsible_df = spark.read.option("header", True).csv(file_path, schema=schema)

    grid_loss_responsible = _get_all_grid_loss_responsible()
    grid_loss_responsible_df = spark.createDataFrame(grid_loss_responsible, schema)

    grid_loss_responsible_df = grid_loss_responsible_df.withColumn(
        Colname.is_positive_grid_loss_responsible, when(col(Colname.metering_point_type) == "consumption", True).otherwise(False))
    grid_loss_responsible_df = grid_loss_responsible_df.withColumn(
        Colname.is_negative_grid_loss_responsible, when(col(Colname.metering_point_type) == "production", True).otherwise(False))

    grid_loss_responsible_df = grid_loss_responsible_df.select(
        col(Colname.metering_point_id),
        col(Colname.grid_area),
        col(Colname.from_date),
        col(Colname.to_date),
        col(Colname.metering_point_type),
        col(Colname.energy_supplier_id),
        col(Colname.is_negative_grid_loss_responsible),
        col(Colname.is_positive_grid_loss_responsible),
    )

    _throw_if_no_grid_loss_responsible(grid_areas, grid_loss_responsible_df)

    return grid_loss_responsible_df


def _throw_if_no_grid_loss_responsible(grid_areas: list[str], grid_loss_responsible_df: DataFrame) -> None:

    for grid_area in grid_areas:
        if grid_loss_responsible_df.filter(col(Colname.grid_area).contains(grid_area)).count() == 0:
            raise ValueError(f"No grid area responsible found for grid area {grid_area}")


def _get_all_grid_loss_responsible() -> list[tuple]:

    default_valid_from = datetime.strptime("2000-01-01T23:00:00+0000", "%Y-%m-%dT%H:%M:%S%z")
    return [
        ('571313180480500149', 804, default_valid_from, None, 'production', '8100000000108'),
        ('570715000000682292', 512, default_valid_from, None, 'production', '5790002437717'),
        ('571313154313676325', 543, default_valid_from, None, 'production', '5790002437717'),
        ('571313153313676335', 533, default_valid_from, None, 'production', '5790002437717'),
        ('571313154391364862', 584, default_valid_from, None, 'production', '5790002437717'),
        ('579900000000000026', 990, default_valid_from, None, 'production', '4260024590017'),
        ('571313180300014979', 803, default_valid_from, None, 'production', '8100000000108'),
        ('571313180400100657', 804, default_valid_from, None, 'consumption', '8100000000115'),
        ('578030000000000012', 803, default_valid_from, None, 'consumption', '8100000000108'),
        ('571313154312753911', 543, default_valid_from, None, 'consumption', '5790001103095'),
        ('571313153308031507', 533, default_valid_from, None, 'consumption', '5790001102357'),
        ('571313158410300060', 584, default_valid_from, None, 'consumption', '5790001103095')
    ]

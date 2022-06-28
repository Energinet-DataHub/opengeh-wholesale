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
from typing import List
from pyspark.sql import SparkSession
from pyspark.sql.types import StructType
from delta.tables import DeltaTable


def create_delta_table_if_empty(spark: SparkSession, delta_table_path: str, schema: StructType, partitionCols: List[str]):
    if DeltaTable.isDeltaTable(spark, delta_table_path):
        return

    emptyDF = spark.createDataFrame(spark.sparkContext.emptyRDD(), schema)
    emptyDF \
        .write \
        .partitionBy(
            partitionCols
        ) \
        .format('delta') \
        .mode('overwrite') \
        .save(delta_table_path)

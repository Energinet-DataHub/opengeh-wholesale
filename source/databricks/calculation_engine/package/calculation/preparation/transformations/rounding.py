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
from pyspark.sql import DataFrame
import pyspark.sql.functions as f


def get_rounded(df: DataFrame) -> DataFrame:
    df = df.orderBy("observation_time")
    df = df.withColumn("index", (f.minute("observation_time") / 15).cast("integer") + 1)

    df = df.withColumn("quantity_row_1", f.col("quantity"))
    df = df.withColumn(
        "quantity_row_2",
        f.col("quantity") - f.round("quantity_row_1", 3) + f.col("quantity_row_1"),
    )
    df = df.withColumn(
        "quantity_row_3",
        f.col("quantity") - f.round("quantity_row_2", 3) + f.col("quantity_row_2"),
    )
    df = df.withColumn(
        "quantity_row_4",
        f.col("quantity") - f.round("quantity_row_3", 3) + f.col("quantity_row_3"),
    )
    df = df.withColumn(
        "round_ready_quantity",
        f.when(f.col("index") == 1, f.col("quantity_row_1"))
        .when(
            f.col("index") == 2,
            f.col("quantity_row_2"),
        )
        .when(f.col("index") == 3, f.col("quantity_row_3"))
        .when(f.col("index") == 4, f.col("quantity_row_4")),
    )
    df = df.withColumn("quantity", f.round(f.col("round_ready_quantity"), 3))

    return df

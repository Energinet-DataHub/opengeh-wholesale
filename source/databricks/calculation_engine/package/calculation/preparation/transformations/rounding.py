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


def special_quantity_rounding(df: DataFrame) -> DataFrame:
    """
    The function rounds the quantity to 3 decimal places.
    All Quantities that come in has a scale of 3.
    The scale will be increased to 6 in different way depending on resolution.
    Quantities with resolution of PT15M get added three zero at the end to increase the scale to 6.
    Quantities with resolution of PT1H gets divided into 4 to act as a PT15M resolution.
    To be able to get the same PT1H quantity result again we have to round to 3 decimal places in a specific way.
    example: if we have a PT1H quantity of 0.003, and we divide it into 4 we get 0.00075.
    if we just round the new value normally we get 0.001. and we add that up 4 times we get 0.004.
    Which is not the same as the original value. To get original value we need to somthing like this:
    Take the first quantity of 0.000750 and round it to 3 decimal places we get 0.001. Now the difference between
    the original value and the rounded value is 0.000750 - 0.001 = -0.000250. We add that to the next quantity.
    0.000750 + (-0.000250) = 0.000500. We round that to 3 decimal places we get 0.001. Now the difference between
    the original value and the rounded value is 0.000500 - 0.001 = -0.000500. We add that to the next quantity.
    0.000750 + (-0.000500) = 0.000250. We round that to 3 decimal places we get 0.000. Now the difference between
    the original value and the rounded value is 0.000250 - 0.000 = 0.000250. We add that to the next quantity.
    0.000750 + 0.000250 = 0.001000. We round that to 3 decimal places we get 0.001.
    Now we can add them up and get the original value of 0.003.
    """
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

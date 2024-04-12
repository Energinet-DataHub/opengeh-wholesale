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
from decimal import Decimal

from pyspark.sql.types import DecimalType

from package.calculation.energy.data_structures.energy_results import EnergyResults
from pyspark.sql import Window, DataFrame
import pyspark.sql.functions as f


def get_rounded(energy_results: EnergyResults) -> DataFrame:
    df = energy_results.df
    # Sort DataFrame by observation_time
    df = df.orderBy("observation_time")

    # Round quantity to 3 decimal places and calculate the difference
    df = df.withColumn("rounded_quantity", f.round(f.col("quantity"), 3))
    df = df.withColumn("diff", f.col("quantity") - f.col("rounded_quantity"))

    # Define window partitioned by hour and ordered by observation_time
    window = Window.partitionBy(f.hour("observation_time")).orderBy("observation_time")

    # Add the diff from the previous row to the current row
    # df = df.withColumn("prev_carry", f.lag("carry_over").over(window))
    # df = df.withColumn(
    #     "diff",
    #     f.when(f.isnull(df.prev_diff), df.diff).otherwise(df.diff + df.prev_diff),
    # )

    # Calculate carry_over and add it to rounded_quantity
    df = df.withColumn("carry_over", f.sum("diff").over(window))
    df = df.withColumn("prev_carry", f.lag("carry_over").over(window))
    df = df.withColumn(
        "prev_carry",
        f.when(f.isnull(df.prev_carry), f.lit(0)).otherwise(df.prev_carry),
    )
    df = df.withColumn("new_quantity", f.col("quantity") + f.col("prev_carry"))
    df = df.withColumn(
        "final_rounded_quantity", f.col("rounded_quantity") + f.col("prev_carry")
    )
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
        "this_quantity",
        f.when(
            f.col("index") == 1,
            f.col("quantity"),
        )
        .when(
            f.col("index") == 2,
            (f.col("quantity") - f.round("quantity", 3)) + f.col("quantity"),
        )
        .when(
            f.col("index") == 3,
            (
                f.col("quantity")
                - f.round(
                    (f.col("quantity") - f.round("quantity", 3)) + f.col("quantity"), 3
                )
            )
            + ((f.col("quantity") - f.round("quantity", 3)) + f.col("quantity")),
        )
        .when(
            f.col("index") == 4,
            f.col("quantity")
            - f.round(
                f.col("quantity")
                - f.round(
                    (f.col("quantity") - f.round("quantity", 3)) + f.col("quantity"), 3
                ),
                3,
            )
            + (
                (
                    f.col("quantity")
                    - f.round(
                        (f.col("quantity") - f.round("quantity", 3))
                        + f.col("quantity"),
                        3,
                    )
                )
                + ((f.col("quantity") - f.round("quantity", 3)) + f.col("quantity"))
            ),
        ),
    )
    df = df.withColumn("quantity_final", f.round(f.col("this_quantity"), 3))

    # Round final_rounded_quantity to 3 decimal places
    df = df.withColumn(
        "final_rounded_quantity", f.round(f.col("final_rounded_quantity"), 3)
    )
    return df

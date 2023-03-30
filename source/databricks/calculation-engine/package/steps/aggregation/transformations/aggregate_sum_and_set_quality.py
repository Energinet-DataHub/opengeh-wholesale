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

from package.constants import Colname
from pyspark.sql import DataFrame
import pyspark.sql.functions as F
from package.codelists import TimeSeriesQuality


def aggregate_sum_and_set_quality(
    result: DataFrame, quantity_col_name: str, group_by: list[str]
) -> DataFrame:
    result = result.na.fill(value=0, subset=[quantity_col_name])
    qualities_col_name = "qualities"
    result = (
        result.groupBy(group_by).agg(
            F.sum(quantity_col_name).alias(Colname.sum_quantity),
            F.collect_set("Quality").alias(qualities_col_name),
        )
        # TODO: What about calculated (A06)?
        .withColumn(
            "Quality",
            F.when(
                F.array_contains(
                    F.col(qualities_col_name), F.lit(TimeSeriesQuality.missing.value)
                )
                | F.array_contains(
                    F.col(qualities_col_name), F.lit(TimeSeriesQuality.incomplete.value)
                ),
                F.lit(TimeSeriesQuality.incomplete.value),
            )
            .when(
                F.array_contains(
                    F.col(qualities_col_name),
                    F.lit(TimeSeriesQuality.estimated.value),
                ),
                F.lit(TimeSeriesQuality.estimated.value),
            )
            .when(
                F.array_contains(
                    F.col(qualities_col_name),
                    F.lit(TimeSeriesQuality.measured.value),
                ),
                F.lit(TimeSeriesQuality.measured.value),
            ),
        )
    )

    return result

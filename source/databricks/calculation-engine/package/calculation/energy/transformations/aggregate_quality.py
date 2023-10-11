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
from pyspark.sql.functions import (
    col,
    when,
    count,
)

from package.codelists import QuantityQuality
from package.constants import Colname

temp_estimated_quality_count = "temp_estimated_quality_count"
temp_quantity_missing_quality_count = "temp_quantity_missing_quality_count"

aggregated_production_quality = "aggregated_production_quality"
aggregated_net_exchange_quality = "aggregated_net_exchange_quality"


def aggregate_total_consumption_quality(df: DataFrame) -> DataFrame:
    df = (
        df.groupBy(Colname.grid_area, Colname.time_window, Colname.sum_quantity)
        .agg(
            # Count entries where quality is estimated
            count(
                when(
                    col(aggregated_production_quality)
                    == QuantityQuality.ESTIMATED.value,
                    1,
                ).when(
                    col(aggregated_net_exchange_quality)
                    == QuantityQuality.ESTIMATED.value,
                    1,
                )
            ).alias(temp_estimated_quality_count),
            # Count entries where quality is quantity missing
            count(
                when(
                    col(aggregated_production_quality) == QuantityQuality.MISSING.value,
                    1,
                ).when(
                    col(aggregated_net_exchange_quality)
                    == QuantityQuality.MISSING.value,
                    1,
                )
            ).alias(temp_quantity_missing_quality_count),
        )
        .withColumn(
            Colname.quality,
            (
                # Set quality to as read if no entries where quality is estimated or quantity missing
                when(
                    col(temp_estimated_quality_count) > 0,
                    QuantityQuality.ESTIMATED.value,
                )
                .when(
                    col(temp_quantity_missing_quality_count) > 0,
                    QuantityQuality.ESTIMATED.value,
                )
                .otherwise(QuantityQuality.MEASURED.value)
            ),
        )
    )
    return df

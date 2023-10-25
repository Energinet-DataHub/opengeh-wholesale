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
import pyspark.sql.functions as f


def aggregate_sum_and_quality(
    result: DataFrame, quantity_col_name: str, group_by: list[str]
) -> DataFrame:
    """
    Aggregates values from metering point time series and groups into an aggregated time-series.

    Sums quantity and collects distinct quality from the metering point time-series.
    """
    return result.groupBy(group_by).agg(
        f.sum(quantity_col_name).alias(Colname.sum_quantity),
        f.collect_set(Colname.quality).alias(Colname.qualities),
    )


def aggregate_sum_and_qualities(
    result: DataFrame, quantity_col_name: str, group_by: list[str]
) -> DataFrame:
    """
    Aggregates values from metering point time series and groups into an aggregated time-series.

    Sums quantity and collects distinct quality from the aggregated time-series.
    """
    return result.groupBy(group_by).agg(
        f.sum(quantity_col_name).alias(Colname.sum_quantity),
        f.array_distinct(f.flatten(f.collect_set(Colname.qualities))).alias(
            Colname.qualities
        ),
    )

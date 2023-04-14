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
import pyspark.sql.functions as F
import pytest

from . import configuration as C
from package.codelists import (
    AggregationLevel,
    TimeSeriesType,
)
from package.constants import Colname


@pytest.mark.parametrize(
    "time_series_type, aggregation_level",
    [
        (
            TimeSeriesType.NET_EXCHANGE_PER_NEIGHBORING_GA.value,
            AggregationLevel.total_ga.value,
        ),
        (
            TimeSeriesType.NET_EXCHANGE_PER_GA.value,
            AggregationLevel.total_ga.value,
        ),
        (
            TimeSeriesType.PRODUCTION.value,
            AggregationLevel.es_per_ga.value,
        ),
        (
            TimeSeriesType.PRODUCTION.value,
            AggregationLevel.brp_per_ga.value,
        ),
        (
            TimeSeriesType.PRODUCTION.value,
            AggregationLevel.total_ga.value,
        ),
        (
            TimeSeriesType.NON_PROFILED_CONSUMPTION.value,
            AggregationLevel.es_per_brp_per_ga.value,
        ),
        (
            TimeSeriesType.NON_PROFILED_CONSUMPTION.value,
            AggregationLevel.es_per_ga.value,
        ),
        (
            TimeSeriesType.NON_PROFILED_CONSUMPTION.value,
            AggregationLevel.brp_per_ga.value,
        ),
        (
            TimeSeriesType.NON_PROFILED_CONSUMPTION.value,
            AggregationLevel.total_ga.value,
        ),
        (
            TimeSeriesType.FLEX_CONSUMPTION.value,
            AggregationLevel.es_per_ga.value,
        ),
        (
            TimeSeriesType.FLEX_CONSUMPTION.value,
            AggregationLevel.brp_per_ga.value,
        ),
        (
            TimeSeriesType.FLEX_CONSUMPTION.value,
            AggregationLevel.total_ga.value,
        ),
    ],
)
def test__result__is_created(
    results_df: DataFrame,
    time_series_type: str,
    aggregation_level: str,
) -> None:
    # Arrange
    result_df = (
        results_df.where(F.col(Colname.batch_id) == C.executed_batch_id)
        .where(F.col(Colname.time_series_type) == time_series_type)
        .where(F.col(Colname.aggregation_level) == aggregation_level)
    )

    # Act: Calculator job is executed just once per session. See the fixtures `results_df` and `executed_calculation_job`

    # Assert: The result is created if there are rows
    assert result_df.count() > 0

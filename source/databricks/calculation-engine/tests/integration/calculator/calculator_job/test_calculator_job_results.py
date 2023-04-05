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

from . import configuration as C
from package.codelists import (
    TimeSeriesType,
)
from package.constants import Colname


def test__net_exchange_per_neighboring_ga__is_created(
    executed_calculation_job: None,
    results_df: DataFrame,
) -> None:
    # Arrange
    result_df = results_df.where(F.col(Colname.batch_id) == C.executed_batch_id).where(
        F.col(Colname.time_series_type)
        == TimeSeriesType.NET_EXCHANGE_PER_NEIGHBORING_GA.value
    )

    # Act: Calculator job is executed just once per session. See the fixture `executed_calculation_job`

    # Assert: The result is created if there are rows
    assert result_df.count() > 0


def test__net_exchange_per_ga__is_created(
    executed_calculation_job: None,
    results_df: DataFrame,
) -> None:
    # Arrange
    result_df = results_df.where(F.col(Colname.batch_id) == C.executed_batch_id).where(
        F.col(Colname.time_series_type) == TimeSeriesType.NET_EXCHANGE_PER_GA.value
    )

    # Act: Calculator job is executed just once per session. See the fixture `executed_calculation_job`

    # Assert: The result is created if there are rows
    assert result_df.count() > 0

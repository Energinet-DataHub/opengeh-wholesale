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
from datetime import datetime

import pytest
from pyspark.sql import SparkSession

from features.utils.factories.basis_data import BasisDataMeteringPointPeriodsFactory
from features.utils.factories.basis_data.basis_data_time_series_points_factory import (
    BasisDataTimeSeriesPointsFactory,
)
from package.infrastructure.paths import (
    BASIS_DATA_DATABASE_NAME,
    METERING_POINT_PERIODS_BASIS_DATA_TABLE_NAME,
    TIME_SERIES_POINTS_BASIS_DATA_TABLE_NAME,
)


@pytest.fixture(scope="session")
def setup_test_data(spark: SparkSession) -> None:
    factory1 = BasisDataMeteringPointPeriodsFactory(spark)
    row = factory1.create_row()
    metering_point_periods = factory1.create_dataframe([row])
    metering_point_periods.write.format("delta").mode("overwrite").saveAsTable(
        f"{BASIS_DATA_DATABASE_NAME}.{METERING_POINT_PERIODS_BASIS_DATA_TABLE_NAME}"
    )

    factory2 = BasisDataTimeSeriesPointsFactory(spark)
    row = factory2.create_row(observation_time=datetime(2020, 1, 1, 23, 0, 0))
    time_series_points = factory2.create_dataframe([row])
    time_series_points.write.format("delta").mode("overwrite").saveAsTable(
        f"{BASIS_DATA_DATABASE_NAME}.{TIME_SERIES_POINTS_BASIS_DATA_TABLE_NAME}"
    )

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
from package.calculation.basis_data.schemas.charge_link_periods_schema import (
    charge_link_periods_schema,
)
from package.calculation.basis_data.schemas.charge_master_data_periods_schema import (
    charge_master_data_periods_schema,
)
from package.calculation.basis_data.schemas.charge_price_points_schema import (
    charge_price_points_schema,
)
from package.calculation.basis_data.schemas.time_series_point_schema import (
    time_series_point_schema,
)
from package.calculation.basis_data.schemas.metering_point_period_schema import (
    metering_point_period_schema,
)
from package.calculation.basis_data.schemas.grid_loss_metering_points_schema import (
    grid_loss_metering_points_schema,
)
from tests.calculation.basis_data.basis_data_test_factory import (
    create_basis_data_factory,
)
import pytest
from pyspark.sql import SparkSession


def test__basis_data_uses_correct_schema(spark: SparkSession):
    basis_data_container = create_basis_data_factory(spark)

    assert (
        basis_data_container.metering_point_periods.schema
        == metering_point_period_schema
    )
    assert basis_data_container.time_series_points.schema == time_series_point_schema
    assert (
        basis_data_container.charge_master_data.schema
        == charge_master_data_periods_schema
    )
    assert basis_data_container.charge_prices.schema == charge_price_points_schema
    assert basis_data_container.charge_links.schema == charge_link_periods_schema
    assert (
        basis_data_container.grid_loss_metering_points.schema
        == grid_loss_metering_points_schema
    )

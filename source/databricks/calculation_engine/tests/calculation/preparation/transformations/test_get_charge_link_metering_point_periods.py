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
from typing import List

import pytest
from pyspark import Row
from pyspark.sql import SparkSession

from calculation.charges_factory import (
    create_charge_link_metering_point_periods,
)
from calculation.input.table_reader import input_metering_point_periods_factory
from calculation.wholesale.factories import (
    input_charge_link_periods_factory,
    input_charge_link_metering_point_periods_factory,
)
from helpers.data_frame_utils import assert_dataframes_equal
from package.calculation.preparation.transformations import (
    get_charge_link_metering_point_periods,
)


@pytest.mark.parametrize(
    "input_metering_point_periods_rows, input_charge_link_periods_rows, expected_rows",
    [
        (
            [
                input_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 2, 23, 0, 0),
                    to_date=datetime(2023, 2, 10, 23, 0, 0),
                )
            ],
            [
                input_charge_link_periods_factory.create_row(
                    from_date=datetime(2023, 2, 2, 23, 0, 0),
                    to_date=datetime(2023, 2, 10, 23, 0, 0),
                )
            ],
            [
                input_charge_link_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 2, 23, 0, 0),
                    to_date=datetime(2023, 2, 10, 23, 0, 0),
                    grid_area=input_metering_point_periods_factory.DEFAULT_GRID_AREA_CODE,
                    charge_type="fee",
                    energy_supplier_id=input_metering_point_periods_factory.DEFAULT_ENERGY_SUPPLIER_ID,
                    metering_point_type="E17",
                )
            ],
        ),
    ],
)
def test_get_charge_link_metering_point_periods(
    spark: SparkSession,
    input_metering_point_periods_rows: list[Row],
    input_charge_link_periods_rows: list[Row],
    expected_rows: List[Row],
) -> None:
    # Arrange
    expected = create_charge_link_metering_point_periods(spark, data=expected_rows)
    input_metering_point_periods = input_metering_point_periods_factory.create(
        spark, data=input_metering_point_periods_rows
    )

    input_charge_link_periods = input_charge_link_periods_factory.create(
        spark, data=input_charge_link_periods_rows
    )

    # Act
    actual = get_charge_link_metering_point_periods(
        input_charge_link_periods, input_metering_point_periods
    )

    # Assert
    assert_dataframes_equal(actual.df, expected.df)

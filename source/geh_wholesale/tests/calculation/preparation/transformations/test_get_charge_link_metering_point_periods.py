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

from geh_wholesale.calculation.preparation.transformations import (
    get_charge_link_metering_point_periods,
)
from tests.calculation.wholesale.factories import (
    prepared_charge_link_metering_point_periods_factory,
    prepared_charge_link_periods_factory,
    prepared_metering_point_periods_factory,
)
from tests.helpers.data_frame_utils import assert_dataframes_equal


@pytest.mark.parametrize(
    "input_metering_point_periods_rows, input_charge_link_periods_rows, expected_rows",
    [
        (
            #   Only a charge link period.
            #   Expected empty result set.
            #   2023-02-02             2023-02-10
            #   CLP |----------------------|
            [],
            [
                prepared_charge_link_periods_factory.create_row(
                    from_date=datetime(2023, 2, 1, 23, 0, 0),
                    to_date=datetime(2023, 2, 9, 23, 0, 0),
                )
            ],
            [],
        ),
        (
            #   Only a metering point period.
            #   Expected empty result set.
            #   2023-02-02             2023-02-10
            #   MPP |----------------------|
            [
                prepared_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 1, 23, 0, 0),
                    to_date=datetime(2023, 2, 9, 23, 0, 0),
                )
            ],
            [],
            [],
        ),
        (
            #   Metering point period and charge link period are identical.
            #   Expected 1 result set.
            #   2023-02-02             2023-02-10
            #   MMP |----------------------|
            #   CLP |----------------------|
            [
                prepared_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 1, 23, 0, 0),
                    to_date=datetime(2023, 2, 9, 23, 0, 0),
                )
            ],
            [
                prepared_charge_link_periods_factory.create_row(
                    from_date=datetime(2023, 2, 1, 23, 0, 0),
                    to_date=datetime(2023, 2, 9, 23, 0, 0),
                )
            ],
            [
                prepared_charge_link_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 1, 23, 0, 0),
                    to_date=datetime(2023, 2, 9, 23, 0, 0),
                )
            ],
        ),
        (
            #   Metering point period and charge link period are staggered.
            #   Expected 1 result set.
            #   2023-02-02             2023-02-10
            #   MMP |----------------------|
            #               2023-02-05            2023-02-13
            #               CLP |----------------------|
            [
                prepared_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 1, 23, 0, 0),
                    to_date=datetime(2023, 2, 9, 23, 0, 0),
                )
            ],
            [
                prepared_charge_link_periods_factory.create_row(
                    from_date=datetime(2023, 2, 4, 23, 0, 0),
                    to_date=datetime(2023, 2, 12, 23, 0, 0),
                )
            ],
            [
                prepared_charge_link_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 4, 23, 0, 0),
                    to_date=datetime(2023, 2, 9, 23, 0, 0),
                )
            ],
        ),
        (
            #   Metering point period and charge link period are staggered.
            #   Expected 1 result set.
            #               2023-02-02             2023-02-10
            #                 MMP |----------------------|
            #   2023-01-25            2023-02-05
            #   CLP |----------------------|
            [
                prepared_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 1, 23, 0, 0),
                    to_date=datetime(2023, 2, 9, 23, 0, 0),
                )
            ],
            [
                prepared_charge_link_periods_factory.create_row(
                    from_date=datetime(2023, 1, 24, 23, 0, 0),
                    to_date=datetime(2023, 2, 4, 23, 0, 0),
                )
            ],
            [
                prepared_charge_link_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 1, 23, 0, 0),
                    to_date=datetime(2023, 2, 4, 23, 0, 0),
                )
            ],
        ),
        (
            #   Metering point period and charge link period don't overlap.
            #   Expected empty result set.
            #                              2023-02-02       2023-02-10
            #                               MMP |---------------|
            #   2023-01-25     2023-01-28
            #   CLP |---------------|
            [
                prepared_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 1, 23, 0, 0),
                    to_date=datetime(2023, 2, 9, 23, 0, 0),
                )
            ],
            [
                prepared_charge_link_periods_factory.create_row(
                    from_date=datetime(2023, 1, 24, 23, 0, 0),
                    to_date=datetime(2023, 1, 27, 23, 0, 0),
                )
            ],
            [],
        ),
        (
            #   Metering point period and charge link period don't overlap.
            #   Expected empty result set.
            #   2023-02-02        2023-02-10
            #   MMP |-----------------|
            #                            2023-02-12     2023-02-28
            #                            CLP |---------------|
            [
                prepared_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 1, 23, 0, 0),
                    to_date=datetime(2023, 2, 9, 23, 0, 0),
                )
            ],
            [
                prepared_charge_link_periods_factory.create_row(
                    from_date=datetime(2023, 2, 21, 23, 0, 0),
                    to_date=datetime(2023, 2, 27, 23, 0, 0),
                )
            ],
            [],
        ),
        (
            #   Metering point period is a subsection of the charge link period.
            #   Expected 1 result set.
            #             2023-02-02        2023-02-10
            #              MMP |-----------------|
            #       2023-01-25                    2023-02-15
            #        CLP |-----------------------------|
            [
                prepared_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 2, 23, 0, 0),
                    to_date=datetime(2023, 2, 10, 23, 0, 0),
                )
            ],
            [
                prepared_charge_link_periods_factory.create_row(
                    from_date=datetime(2023, 1, 25, 23, 0, 0),
                    to_date=datetime(2023, 2, 14, 23, 0, 0),
                )
            ],
            [
                prepared_charge_link_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 2, 23, 0, 0),
                    to_date=datetime(2023, 2, 10, 23, 0, 0),
                )
            ],
        ),
        (
            #   Charge link period is a subsection of the metering point period.
            #   Expected 1 result set.
            #             2023-02-02        2023-02-10
            #              MMP |-----------------|
            #                 2023-02-04  2023-02-08
            #                  CLP |----------|
            [
                prepared_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 2, 23, 0, 0),
                    to_date=datetime(2023, 2, 10, 23, 0, 0),
                )
            ],
            [
                prepared_charge_link_periods_factory.create_row(
                    from_date=datetime(2023, 2, 4, 23, 0, 0),
                    to_date=datetime(2023, 2, 7, 23, 0, 0),
                )
            ],
            [
                prepared_charge_link_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 4, 23, 0, 0),
                    to_date=datetime(2023, 2, 7, 23, 0, 0),
                )
            ],
        ),
        (
            #   2 metering point periods and 1 charge link period.
            #   Expected 2 result sets.
            #   2023-02-02        2023-02-10        2023-02-18
            #   MMP |-----------------|-----------------|
            #   CLP |-----------------------------------|
            [
                prepared_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 1, 23, 0, 0),
                    to_date=datetime(2023, 2, 9, 23, 0, 0),
                ),
                prepared_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 9, 23, 0, 0),
                    to_date=datetime(2023, 2, 17, 23, 0, 0),
                ),
            ],
            [
                prepared_charge_link_periods_factory.create_row(
                    from_date=datetime(2023, 2, 1, 23, 0, 0),
                    to_date=datetime(2023, 2, 17, 23, 0, 0),
                )
            ],
            [
                prepared_charge_link_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 1, 23, 0, 0),
                    to_date=datetime(2023, 2, 9, 23, 0, 0),
                ),
                prepared_charge_link_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 9, 23, 0, 0),
                    to_date=datetime(2023, 2, 17, 23, 0, 0),
                ),
            ],
        ),
        (
            #   1 metering point period and 2 charge link periods.
            #   Expected 2 result sets.
            #   2023-02-02                                         2023-02-18
            #   MMP |--------------------------------------------------|
            #      2023-02-04   2023-02-06   2023-02-08    2023-02-10
            #   CLP    |------------|            |-------------|
            [
                prepared_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 2, 23, 0, 0),
                    to_date=datetime(2023, 2, 17, 23, 0, 0),
                )
            ],
            [
                prepared_charge_link_periods_factory.create_row(
                    from_date=datetime(2023, 2, 4, 23, 0, 0),
                    to_date=datetime(2023, 2, 5, 23, 0, 0),
                ),
                prepared_charge_link_periods_factory.create_row(
                    from_date=datetime(2023, 2, 8, 23, 0, 0),
                    to_date=datetime(2023, 2, 9, 23, 0, 0),
                ),
            ],
            [
                prepared_charge_link_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 4, 23, 0, 0),
                    to_date=datetime(2023, 2, 5, 23, 0, 0),
                ),
                prepared_charge_link_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 8, 23, 0, 0),
                    to_date=datetime(2023, 2, 9, 23, 0, 0),
                ),
            ],
        ),
        (
            #   2 metering point periods and 1 charge link period.
            #   Expected 2 result sets.
            #       2023-02-04   2023-02-06   2023-02-08    2023-02-10
            #   MMP     |------------|            |-------------|
            #
            #              2023-02-05                2023-02-09
            #   CLP            |-------------------------|
            [
                prepared_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 3, 23, 0, 0),
                    to_date=datetime(2023, 2, 5, 23, 0, 0),
                ),
                prepared_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 7, 23, 0, 0),
                    to_date=datetime(2023, 2, 9, 23, 0, 0),
                ),
            ],
            [
                prepared_charge_link_periods_factory.create_row(
                    from_date=datetime(2023, 2, 4, 23, 0, 0),
                    to_date=datetime(2023, 2, 8, 23, 0, 0),
                )
            ],
            [
                prepared_charge_link_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 4, 23, 0, 0),
                    to_date=datetime(2023, 2, 5, 23, 0, 0),
                ),
                prepared_charge_link_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 7, 23, 0, 0),
                    to_date=datetime(2023, 2, 8, 23, 0, 0),
                ),
            ],
        ),
        (
            #   1 metering point periods and 2 charge link period.
            #   Expected 2 result sets.
            #   2023-02-02          2023-02-06            2023-02-10
            #   MMP |------------------------------------------|
            #   CLP |--------------------|---------------------|
            [
                prepared_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 1, 23, 0, 0),
                    to_date=datetime(2023, 2, 9, 23, 0, 0),
                )
            ],
            [
                prepared_charge_link_periods_factory.create_row(
                    from_date=datetime(2023, 2, 1, 23, 0, 0),
                    to_date=datetime(2023, 2, 5, 23, 0, 0),
                ),
                prepared_charge_link_periods_factory.create_row(
                    from_date=datetime(2023, 2, 5, 23, 0, 0),
                    to_date=datetime(2023, 2, 9, 23, 0, 0),
                ),
            ],
            [
                prepared_charge_link_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 1, 23, 0, 0),
                    to_date=datetime(2023, 2, 5, 23, 0, 0),
                ),
                prepared_charge_link_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 5, 23, 0, 0),
                    to_date=datetime(2023, 2, 9, 23, 0, 0),
                ),
            ],
        ),
        (
            #   2 metering point periods and 1 charge link period.
            #   Expected empty result set.
            #   2023-02-02      2023-02-04     2023-02-08       2023-02-10
            #   MMP |----------------|              |----------------|
            #   CLP                  |--------------|
            [
                prepared_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 1, 23, 0, 0),
                    to_date=datetime(2023, 2, 3, 23, 0, 0),
                ),
                prepared_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 7, 23, 0, 0),
                    to_date=datetime(2023, 2, 9, 23, 0, 0),
                ),
            ],
            [
                prepared_charge_link_periods_factory.create_row(
                    from_date=datetime(2023, 2, 3, 23, 0, 0),
                    to_date=datetime(2023, 2, 7, 23, 0, 0),
                ),
            ],
            [],
        ),
        (
            #   4 metering point periods and 1 charge link period.
            #   Expected 1 result set.
            #   2023-02-02      2023-02-04     2023-02-08       2023-02-12
            #   MMP |----------------|--------------|----------------|
            #   CLP |------------------------------------------------|
            [
                prepared_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 1, 23, 0, 0),
                    to_date=datetime(2023, 2, 3, 23, 0, 0),
                ),
                prepared_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 3, 23, 0, 0),
                    to_date=datetime(2023, 2, 7, 23, 0, 0),
                ),
                prepared_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 7, 23, 0, 0),
                    to_date=datetime(2023, 2, 11, 23, 0, 0),
                ),
            ],
            [
                prepared_charge_link_periods_factory.create_row(
                    from_date=datetime(2023, 2, 1, 23, 0, 0),
                    to_date=datetime(2023, 2, 11, 23, 0, 0),
                ),
            ],
            [
                prepared_charge_link_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 1, 23, 0, 0),
                    to_date=datetime(2023, 2, 3, 23, 0, 0),
                ),
                prepared_charge_link_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 3, 23, 0, 0),
                    to_date=datetime(2023, 2, 7, 23, 0, 0),
                ),
                prepared_charge_link_metering_point_periods_factory.create_row(
                    from_date=datetime(2023, 2, 7, 23, 0, 0),
                    to_date=datetime(2023, 2, 11, 23, 0, 0),
                ),
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
    expected = prepared_charge_link_metering_point_periods_factory.create(spark, data=expected_rows)

    input_metering_point_periods = prepared_metering_point_periods_factory.create(
        spark, data=input_metering_point_periods_rows
    )

    input_charge_link_periods = prepared_charge_link_periods_factory.create(spark, data=input_charge_link_periods_rows)

    # Act
    actual = get_charge_link_metering_point_periods(input_charge_link_periods, input_metering_point_periods)

    # Assert
    assert_dataframes_equal(actual.df, expected)

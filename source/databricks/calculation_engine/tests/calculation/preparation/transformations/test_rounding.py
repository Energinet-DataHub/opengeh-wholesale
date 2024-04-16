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
from decimal import Decimal

from pyspark.sql import SparkSession
from package.calculation.preparation.transformations.rounding import (
    special_quantity_rounding,
)
from package.constants import Colname


def test_get_rounded(spark: SparkSession) -> None:
    # Arrange
    rows = [
        {
            Colname.observation_time: datetime(2022, 1, 1, 1, 0),
            Colname.quantity: Decimal("222.03075"),
        },
        {
            Colname.observation_time: datetime(2022, 1, 1, 1, 15),
            Colname.quantity: Decimal("222.03075"),
        },
        {
            Colname.observation_time: datetime(2022, 1, 1, 1, 30),
            Colname.quantity: Decimal("222.03075"),
        },
        {
            Colname.observation_time: datetime(2022, 1, 1, 1, 45),
            Colname.quantity: Decimal("222.03075"),
        },
    ]
    df = spark.createDataFrame(rows)

    # Act
    actual = special_quantity_rounding(df)
    actual.show()

    # Assert
    assert actual.collect()[0][Colname.quantity] == Decimal("222.031")
    assert actual.collect()[1][Colname.quantity] == Decimal("222.031")
    assert actual.collect()[2][Colname.quantity] == Decimal("222.030")
    assert actual.collect()[3][Colname.quantity] == Decimal("222.031")

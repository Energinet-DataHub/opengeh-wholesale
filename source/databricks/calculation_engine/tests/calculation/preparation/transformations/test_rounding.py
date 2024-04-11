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
from pyspark.sql.functions import lit
from pyspark.sql.types import DecimalType

import tests.calculation.energy.energy_results_factories as factory
from package.calculation.energy.data_structures.energy_results import (
    EnergyResults,
    energy_results_schema,
)
from package.calculation.preparation.transformations.rounding import get_rounded
from package.constants import Colname


def test_get_rounded(spark: SparkSession) -> None:
    # Arrange
    rows = [
        factory.create_row(
            quantity=Decimal("0.000750"), observation_time=datetime(2022, 1, 1, 1, 0)
        ),
        factory.create_row(
            quantity=Decimal("0.000750"), observation_time=datetime(2022, 1, 1, 1, 15)
        ),
        factory.create_row(
            quantity=Decimal("0.000750"), observation_time=datetime(2022, 1, 1, 1, 30)
        ),
        factory.create_row(
            quantity=Decimal("0.00075"), observation_time=datetime(2022, 1, 1, 1, 45)
        ),
        factory.create_row(
            quantity=Decimal("0.002000"), observation_time=datetime(2022, 1, 1, 2, 0)
        ),
        factory.create_row(
            quantity=Decimal("0.002000"), observation_time=datetime(2022, 1, 1, 2, 15)
        ),
        factory.create_row(
            quantity=Decimal("0.002000"), observation_time=datetime(2022, 1, 1, 2, 30)
        ),
        factory.create_row(
            quantity=Decimal("0.002000"), observation_time=datetime(2022, 1, 1, 2, 45)
        ),
    ]
    df = factory.create(spark, data=rows)
    df.df.show()
    # Act
    actual = get_rounded(df)
    actual.show()

    # Assert

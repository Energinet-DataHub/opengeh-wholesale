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
from typing import Callable

import pandas as pd
import pytest
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.types import (
    StructType,
    StringType,
    TimestampType,
    BooleanType,
)

from package.calculation.energy.transformations import adjust_production
from package.codelists import (
    MeteringPointType,
    QuantityQuality,
)
from package.constants import Colname
import tests.calculation.energy.energy_results_factories as factories

# Default values
default_domain = "D1"
default_responsible = "R1"
default_supplier = "S1"
default_sum_quantity = Decimal(1)
default_negative_grid_loss = Decimal(3)
default_aggregated_quality = QuantityQuality.ESTIMATED.value
default_metering_point_type = MeteringPointType.PRODUCTION.value

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_time_window = {
    Colname.start: datetime(2020, 1, 1, 0, 0),
    Colname.end: datetime(2020, 1, 1, 1, 0),
}
default_valid_from = datetime.strptime(
    "2020-01-01T00:00:00+0000", date_time_formatting_string
)
default_valid_to = datetime.strptime(
    "2020-01-01T01:00:00+0000", date_time_formatting_string
)


class TestWhenValidInput:
    @pytest.fixture(scope="module")
    def sys_cor_schema(self) -> StructType:
        """
        Input system correction data frame schema
        """
        return (
            StructType()
            .add(Colname.grid_area, StringType(), False)
            .add(Colname.balance_responsible_id, StringType())
            .add(Colname.energy_supplier_id, StringType())
            .add(Colname.from_date, TimestampType())
            .add(Colname.to_date, TimestampType())
            .add(Colname.is_negative_grid_loss_responsible, BooleanType())
        )

    @pytest.fixture(scope="module")
    def sys_cor_row_factory(
        self, spark: SparkSession, sys_cor_schema: StructType
    ) -> Callable[..., DataFrame]:
        """
        Factory to generate a single row of  data, with default parameters as specified above.
        """

        def factory(
            domain: str = default_domain,
            responsible: str = default_responsible,
            supplier: str = default_supplier,
            valid_from: datetime = default_valid_from,
            valid_to: datetime = default_valid_to,
            is_negative_grid_loss_responsible: bool = True,
        ) -> DataFrame:
            pandas_df = pd.DataFrame(
                {
                    Colname.grid_area: [domain],
                    Colname.balance_responsible_id: [responsible],
                    Colname.energy_supplier_id: [supplier],
                    Colname.from_date: [valid_from],
                    Colname.to_date: [valid_to],
                    Colname.is_negative_grid_loss_responsible: [
                        is_negative_grid_loss_responsible
                    ],
                }
            )
            return spark.createDataFrame(pandas_df, schema=sys_cor_schema)

        return factory

    def test__adjust_production__returns_qualities_from_hourly_production_and_negative_grid_loss(
        self,
        spark: SparkSession,
        sys_cor_row_factory: Callable[..., DataFrame],
    ) -> None:
        # Arrange
        expected_qualities = [
            QuantityQuality.CALCULATED.value,
            QuantityQuality.ESTIMATED.value,
        ]

        production_row = factories.create_row(
            qualities=[QuantityQuality.CALCULATED],
            metering_point_type=MeteringPointType.PRODUCTION,
        )
        production = factories.create(spark, [production_row])

        negative_grid_loss_row = factories.create_row(
            qualities=[QuantityQuality.ESTIMATED],
            metering_point_type=MeteringPointType.PRODUCTION,
        )
        negative_grid_loss = factories.create(spark, [negative_grid_loss_row])

        grid_loss_sys_cor_master_data = sys_cor_row_factory()

        # Act
        actual = adjust_production(
            production, negative_grid_loss, grid_loss_sys_cor_master_data
        )

        # Assert
        actual_row = actual.df.collect()[0]
        actual_qualities = actual_row[Colname.qualities]
        assert set(actual_qualities) == set(expected_qualities)

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
from pyspark.sql.functions import col
from pyspark.sql.types import (
    StructType,
    StringType,
    DecimalType,
    TimestampType,
    BooleanType,
    ArrayType,
)

from package.calculation.energy.aggregators.grid_loss_aggregators import (
    apply_grid_loss_adjustment,
)
from package.calculation.energy.energy_results import (
    EnergyResults,
    energy_results_schema,
)
from package.codelists import (
    MeteringPointType,
    QuantityQuality,
)
from package.constants import Colname
from package.calculation.preparation.grid_loss_responsible import GridLossResponsible

# Default values
default_metering_point_id = "1234567890123"
default_domain = "D1"
default_responsible = "R1"
default_supplier = "S1"
default_sum_quantity = Decimal(1)
default_positive_grid_loss = Decimal(3)
default_aggregated_quality = QuantityQuality.ESTIMATED.value
default_metering_point_type = MeteringPointType.CONSUMPTION.value

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


@pytest.fixture(scope="module")
def grid_loss_sys_cor_schema() -> StructType:
    """
    Input grid loss system correction data frame schema
    """
    return (
        StructType()
        .add(Colname.metering_point_id, StringType(), False)
        .add(Colname.grid_area, StringType(), False)
        .add(Colname.metering_point_type, StringType(), False)
        .add(Colname.balance_responsible_id, StringType())
        .add(Colname.energy_supplier_id, StringType())
        .add(Colname.from_date, TimestampType())
        .add(Colname.to_date, TimestampType())
        .add(Colname.is_positive_grid_loss_responsible, BooleanType())
    )


@pytest.fixture(scope="module")
def flex_consumption_result_row_factory(
    spark: SparkSession,
) -> Callable[..., EnergyResults]:
    """
    Factory to generate a single row of  data, with default parameters as specified above.
    """

    def factory(
        domain: str = default_domain,
        responsible: str = default_responsible,
        supplier: str = default_supplier,
        sum_quantity: Decimal = default_sum_quantity,
        time_window=None,
        aggregated_quality: str = default_aggregated_quality,
    ) -> EnergyResults:
        if time_window is None:
            time_window = default_time_window
        pandas_df = pd.DataFrame(
            {
                Colname.grid_area: [domain],
                Colname.to_grid_area: [None],
                Colname.from_grid_area: [None],
                Colname.balance_responsible_id: [responsible],
                Colname.energy_supplier_id: [supplier],
                Colname.time_window: [time_window],
                Colname.sum_quantity: [sum_quantity],
                Colname.qualities: [[aggregated_quality]],
            }
        )
        df = spark.createDataFrame(pandas_df, schema=energy_results_schema)
        return EnergyResults(df)

    return factory


@pytest.fixture(scope="module")
def positive_grid_loss_result_row_factory(
    spark: SparkSession,
) -> Callable[..., EnergyResults]:
    """
    Factory to generate a single row of  data, with default parameters as specified above.
    """

    def factory(
        domain: str = default_domain,
        time_window=None,
        positive_grid_loss: Decimal = default_positive_grid_loss,
        aggregated_quality: str = default_aggregated_quality,
    ) -> EnergyResults:
        if time_window is None:
            time_window = default_time_window
        pandas_df = pd.DataFrame(
            {
                Colname.grid_area: [domain],
                Colname.to_grid_area: [None],
                Colname.from_grid_area: [None],
                Colname.balance_responsible_id: ["balance_responsible_id"],
                Colname.energy_supplier_id: ["energy_supplier_id"],
                Colname.time_window: [time_window],
                Colname.sum_quantity: [positive_grid_loss],
                Colname.qualities: [[aggregated_quality]],
            }
        )
        df = spark.createDataFrame(pandas_df, schema=energy_results_schema)
        return EnergyResults(df)

    return factory


@pytest.fixture(scope="module")
def grid_loss_sys_cor_row_factory(
    spark: SparkSession, grid_loss_sys_cor_schema: StructType
) -> Callable[..., GridLossResponsible]:
    """
    Factory to generate a single row of  data, with default parameters as specified above.
    """

    def factory(
        metering_point_id: str = default_metering_point_id,
        domain: str = default_domain,
        metering_point_type: str = default_metering_point_type,
        responsible: str = default_responsible,
        supplier: str = default_supplier,
        valid_from: datetime = default_valid_from,
        valid_to: datetime = default_valid_to,
        is_positive_grid_loss_responsible: bool = True,
    ) -> GridLossResponsible:
        pandas_df = pd.DataFrame(
            {
                metering_point_id: [metering_point_id],
                Colname.grid_area: [domain],
                Colname.metering_point_type: [metering_point_type],
                Colname.balance_responsible_id: [responsible],
                Colname.energy_supplier_id: [supplier],
                Colname.from_date: [valid_from],
                Colname.to_date: [valid_to],
                Colname.is_positive_grid_loss_responsible: [
                    is_positive_grid_loss_responsible
                ],
            }
        )
        df = spark.createDataFrame(pandas_df, schema=grid_loss_sys_cor_schema)

        return GridLossResponsible(df)

    return factory


def test_grid_area_grid_loss_is_added_to_grid_loss_energy_responsible(
    flex_consumption_result_row_factory: Callable[..., EnergyResults],
    positive_grid_loss_result_row_factory: Callable[..., EnergyResults],
    grid_loss_sys_cor_row_factory: Callable[..., GridLossResponsible],
) -> None:
    # Arrange
    flex_consumption = flex_consumption_result_row_factory(supplier="A")
    positive_grid_loss = positive_grid_loss_result_row_factory()
    grid_loss_sys_cor_master_data = grid_loss_sys_cor_row_factory(supplier="A")

    # Act
    actual = apply_grid_loss_adjustment(
        flex_consumption,
        positive_grid_loss,
        grid_loss_sys_cor_master_data,
        MeteringPointType.CONSUMPTION,
    )

    # Assert
    assert (
        actual.df.where(col(Colname.energy_supplier_id) == "A").collect()[0][
            Colname.sum_quantity
        ]
        == default_positive_grid_loss + default_sum_quantity
    )


def test_grid_area_grid_loss_is_not_added_to_non_grid_loss_energy_responsible(
    flex_consumption_result_row_factory: Callable[..., EnergyResults],
    positive_grid_loss_result_row_factory: Callable[..., EnergyResults],
    grid_loss_sys_cor_row_factory: Callable[..., GridLossResponsible],
) -> None:
    # Arrange
    flex_consumption = flex_consumption_result_row_factory(supplier="A")
    positive_grid_loss = positive_grid_loss_result_row_factory()
    grid_loss_sys_cor_master_data = grid_loss_sys_cor_row_factory(supplier="B")

    # Act
    actual = apply_grid_loss_adjustment(
        flex_consumption,
        positive_grid_loss,
        grid_loss_sys_cor_master_data,
        MeteringPointType.CONSUMPTION,
    )

    # Assert
    assert (
        actual.df.where(col(Colname.energy_supplier_id) == "A").collect()[0][
            Colname.sum_quantity
        ]
        == default_sum_quantity
    )


def test_result_dataframe_contains_same_number_of_results_with_same_energy_suppliers_as_flex_consumption_result_dataframe(
    flex_consumption_result_row_factory: Callable[..., EnergyResults],
    positive_grid_loss_result_row_factory: Callable[..., EnergyResults],
    grid_loss_sys_cor_row_factory: Callable[..., GridLossResponsible],
) -> None:
    # Arrange
    fc_row_1 = flex_consumption_result_row_factory(supplier="A")
    fc_row_2 = flex_consumption_result_row_factory(supplier="B")
    fc_row_3 = flex_consumption_result_row_factory(supplier="C")

    flex_consumption = EnergyResults(fc_row_1.df.union(fc_row_2.df).union(fc_row_3.df))
    positive_grid_loss = positive_grid_loss_result_row_factory()
    grid_loss_sys_cor_master_data = grid_loss_sys_cor_row_factory(supplier="C")

    # Act
    actual = apply_grid_loss_adjustment(
        flex_consumption,
        positive_grid_loss,
        grid_loss_sys_cor_master_data,
        MeteringPointType.CONSUMPTION,
    )

    # Assert
    actual_collect = actual.df.collect()
    assert actual.df.count() == 3
    assert actual_collect[0][Colname.energy_supplier_id] == "A"
    assert actual_collect[1][Colname.energy_supplier_id] == "B"
    assert actual_collect[2][Colname.energy_supplier_id] == "C"

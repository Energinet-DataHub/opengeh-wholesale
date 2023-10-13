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
from decimal import Decimal
from datetime import datetime
from package.codelists import (
    MeteringPointType,
    MeteringPointResolution,
    QuantityQuality,
)
from package.calculation.energy.transformations import adjust_flex_consumption
from pyspark.sql.functions import col
from pyspark.sql.types import (
    StructType,
    StringType,
    DecimalType,
    TimestampType,
    BooleanType,
)
import pytest
import pandas as pd
from package.constants import Colname
from pyspark.sql import SparkSession, DataFrame
from typing import Callable

# Default values
default_domain = "D1"
default_responsible = "R1"
default_supplier = "S1"
default_sum_quantity = Decimal(1)
default_positive_grid_loss = Decimal(3)
default_aggregated_quality = QuantityQuality.ESTIMATED.value
default_resolution = MeteringPointResolution.HOUR.value
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
def flex_consumption_result_schema() -> StructType:
    """
    Input flex consumption result data frame schema
    """
    return (
        StructType()
        .add(Colname.grid_area, StringType(), False)
        .add(Colname.balance_responsible_id, StringType())
        .add(Colname.energy_supplier_id, StringType())
        .add(Colname.sum_quantity, DecimalType())
        .add(
            Colname.time_window,
            StructType()
            .add(Colname.start, TimestampType())
            .add(Colname.end, TimestampType()),
            False,
        )
        .add(Colname.quality, StringType())
        .add(Colname.resolution, StringType())
        .add(Colname.metering_point_type, StringType())
    )


@pytest.fixture(scope="module")
def positive_grid_loss_result_schema() -> StructType:
    """
    Input grid loss result schema
    """
    return (
        StructType()
        .add(Colname.grid_area, StringType(), False)
        .add(
            Colname.time_window,
            StructType()
            .add(Colname.start, TimestampType())
            .add(Colname.end, TimestampType()),
            False,
        )
        .add(Colname.sum_quantity, DecimalType())
        .add(Colname.quality, StringType())
        .add(Colname.resolution, StringType())
        .add(Colname.metering_point_type, StringType())
    )


@pytest.fixture(scope="module")
def grid_loss_sys_cor_schema() -> StructType:
    """
    Input grid loss system correction data frame schema
    """
    return (
        StructType()
        .add(Colname.grid_area, StringType(), False)
        .add(Colname.balance_responsible_id, StringType())
        .add(Colname.energy_supplier_id, StringType())
        .add(Colname.from_date, TimestampType())
        .add(Colname.to_date, TimestampType())
        .add(Colname.is_positive_grid_loss_responsible, BooleanType())
    )


@pytest.fixture(scope="module")
def expected_schema() -> StructType:
    """
    Expected aggregation schema
    NOTE: Spark seems to add 10 to the precision of the decimal type on summations.
    Thus, the expected schema should be precision of 20, 10 more than the default of 10.
    If this is an issue we can always cast back to the original decimal precision in the aggregate
    function.
    https://stackoverflow.com/questions/57203383/spark-sum-and-decimaltype-precision
    """
    return (
        StructType()
        .add(Colname.grid_area, StringType(), False)
        .add(Colname.balance_responsible_id, StringType())
        .add(Colname.energy_supplier_id, StringType())
        .add(
            Colname.time_window,
            StructType()
            .add(Colname.start, TimestampType())
            .add(Colname.end, TimestampType()),
            False,
        )
        .add(Colname.sum_quantity, DecimalType())
        .add(Colname.quality, StringType())
    )


@pytest.fixture(scope="module")
def flex_consumption_result_row_factory(
    spark: SparkSession, flex_consumption_result_schema: StructType
) -> Callable[..., DataFrame]:
    """
    Factory to generate a single row of  data, with default parameters as specified above.
    """

    def factory(
        domain: str = default_domain,
        responsible: str = default_responsible,
        supplier: str = default_supplier,
        sum_quantity: Decimal = default_sum_quantity,
        time_window: dict[str, datetime] = default_time_window,
        aggregated_quality: str = default_aggregated_quality,
        resolution: str = default_resolution,
        metering_point_type: str = default_metering_point_type,
    ) -> DataFrame:
        pandas_df = pd.DataFrame(
            {
                Colname.grid_area: [domain],
                Colname.balance_responsible_id: [responsible],
                Colname.energy_supplier_id: [supplier],
                Colname.sum_quantity: [sum_quantity],
                Colname.time_window: [time_window],
                Colname.quality: [aggregated_quality],
                Colname.resolution: [resolution],
                Colname.metering_point_type: [metering_point_type],
            }
        )
        return spark.createDataFrame(pandas_df, schema=flex_consumption_result_schema)

    return factory


@pytest.fixture(scope="module")
def positive_grid_loss_result_row_factory(
    spark: SparkSession, positive_grid_loss_result_schema: StructType
) -> Callable[..., DataFrame]:
    """
    Factory to generate a single row of  data, with default parameters as specified above.
    """

    def factory(
        domain: str = default_domain,
        time_window: dict[str, datetime] = default_time_window,
        positive_grid_loss: Decimal = default_positive_grid_loss,
        aggregated_quality: str = default_aggregated_quality,
        resolution: str = default_resolution,
        metering_point_type: str = default_metering_point_type,
    ) -> DataFrame:
        pandas_df = pd.DataFrame(
            {
                Colname.grid_area: [domain],
                Colname.time_window: [time_window],
                Colname.sum_quantity: [positive_grid_loss],
                Colname.quality: [aggregated_quality],
                Colname.resolution: [resolution],
                Colname.metering_point_type: [metering_point_type],
            }
        )
        return spark.createDataFrame(pandas_df, schema=positive_grid_loss_result_schema)

    return factory


@pytest.fixture(scope="module")
def grid_loss_sys_cor_row_factory(
    spark: SparkSession, grid_loss_sys_cor_schema: StructType
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
        is_positive_grid_loss_responsible: bool = True,
    ) -> DataFrame:
        pandas_df = pd.DataFrame(
            {
                Colname.grid_area: [domain],
                Colname.balance_responsible_id: [responsible],
                Colname.energy_supplier_id: [supplier],
                Colname.from_date: [valid_from],
                Colname.to_date: [valid_to],
                Colname.is_positive_grid_loss_responsible: [
                    is_positive_grid_loss_responsible
                ],
            }
        )
        return spark.createDataFrame(pandas_df, schema=grid_loss_sys_cor_schema)

    return factory


def test_grid_area_grid_loss_is_added_to_grid_loss_energy_responsible(
    flex_consumption_result_row_factory: Callable[..., DataFrame],
    positive_grid_loss_result_row_factory: Callable[..., DataFrame],
    grid_loss_sys_cor_row_factory: Callable[..., DataFrame],
) -> None:
    flex_consumption = flex_consumption_result_row_factory(supplier="A")
    positive_grid_loss = positive_grid_loss_result_row_factory()
    grid_loss_sys_cor_master_data = grid_loss_sys_cor_row_factory(supplier="A")

    result_df = adjust_flex_consumption(
        flex_consumption, positive_grid_loss, grid_loss_sys_cor_master_data
    )

    assert (
        result_df.filter(col(Colname.energy_supplier_id) == "A").collect()[0][
            Colname.sum_quantity
        ]
        == default_positive_grid_loss + default_sum_quantity
    )


def test_grid_area_grid_loss_is_not_added_to_non_grid_loss_energy_responsible(
    flex_consumption_result_row_factory: Callable[..., DataFrame],
    positive_grid_loss_result_row_factory: Callable[..., DataFrame],
    grid_loss_sys_cor_row_factory: Callable[..., DataFrame],
) -> None:
    flex_consumption = flex_consumption_result_row_factory(supplier="A")
    positive_grid_loss = positive_grid_loss_result_row_factory()
    grid_loss_sys_cor_master_data = grid_loss_sys_cor_row_factory(supplier="B")

    result_df = adjust_flex_consumption(
        flex_consumption, positive_grid_loss, grid_loss_sys_cor_master_data
    )

    assert (
        result_df.filter(col(Colname.energy_supplier_id) == "A").collect()[0][
            Colname.sum_quantity
        ]
        == default_sum_quantity
    )


def test_result_dataframe_contains_same_number_of_results_with_same_energy_suppliers_as_flex_consumption_result_dataframe(
    flex_consumption_result_row_factory: Callable[..., DataFrame],
    positive_grid_loss_result_row_factory: Callable[..., DataFrame],
    grid_loss_sys_cor_row_factory: Callable[..., DataFrame],
) -> None:
    fc_row_1 = flex_consumption_result_row_factory(supplier="A")
    fc_row_2 = flex_consumption_result_row_factory(supplier="B")
    fc_row_3 = flex_consumption_result_row_factory(supplier="C")

    flex_consumption = fc_row_1.union(fc_row_2).union(fc_row_3)
    positive_grid_loss = positive_grid_loss_result_row_factory()
    grid_loss_sys_cor_master_data = grid_loss_sys_cor_row_factory(supplier="C")

    result_df = adjust_flex_consumption(
        flex_consumption, positive_grid_loss, grid_loss_sys_cor_master_data
    )

    result_df_collect = result_df.collect()
    assert result_df.count() == 3
    assert result_df_collect[0][Colname.energy_supplier_id] == "A"
    assert result_df_collect[1][Colname.energy_supplier_id] == "B"
    assert result_df_collect[2][Colname.energy_supplier_id] == "C"


def test_correct_grid_loss_entry_is_used_to_determine_energy_responsible_for_the_given_time_window_from_flex_consumption_result_dataframe(
    flex_consumption_result_row_factory: Callable[..., DataFrame],
    positive_grid_loss_result_row_factory: Callable[..., DataFrame],
    grid_loss_sys_cor_row_factory: Callable[..., DataFrame],
) -> None:
    time_window_1 = {
        Colname.start: datetime(2020, 1, 1, 0, 0),
        Colname.end: datetime(2020, 1, 1, 1, 0),
    }
    time_window_2 = {
        Colname.start: datetime(2020, 1, 1, 1, 0),
        Colname.end: datetime(2020, 1, 1, 2, 0),
    }
    time_window_3 = {
        Colname.start: datetime(2020, 1, 1, 2, 0),
        Colname.end: datetime(2020, 1, 1, 3, 0),
    }

    fc_row_1 = flex_consumption_result_row_factory(
        supplier="A", time_window=time_window_1
    )
    fc_row_2 = flex_consumption_result_row_factory(
        supplier="B", time_window=time_window_2
    )
    fc_row_3 = flex_consumption_result_row_factory(
        supplier="B", time_window=time_window_3
    )

    flex_consumption = fc_row_1.union(fc_row_2).union(fc_row_3)

    gagl_result_1 = Decimal(1)
    gagl_result_2 = Decimal(2)
    gagl_result_3 = Decimal(3)

    gagl_row_1 = positive_grid_loss_result_row_factory(
        time_window=time_window_1, positive_grid_loss=gagl_result_1
    )
    gagl_row_2 = positive_grid_loss_result_row_factory(
        time_window=time_window_2, positive_grid_loss=gagl_result_2
    )
    gagl_row_3 = positive_grid_loss_result_row_factory(
        time_window=time_window_3, positive_grid_loss=gagl_result_3
    )

    positive_grid_loss = gagl_row_1.union(gagl_row_2).union(gagl_row_3)

    glsc_row_1 = grid_loss_sys_cor_row_factory(
        supplier="A", valid_from=time_window_1["start"], valid_to=time_window_1["end"]
    )
    glsc_row_2 = grid_loss_sys_cor_row_factory(
        supplier="C", valid_from=time_window_2["start"], valid_to=time_window_2["end"]
    )
    glsc_row_3 = grid_loss_sys_cor_row_factory(
        supplier="B", valid_from=time_window_3["start"], valid_to=None
    )

    grid_loss_sys_cor_master_data = glsc_row_1.union(glsc_row_2).union(glsc_row_3)

    result_df = adjust_flex_consumption(
        flex_consumption, positive_grid_loss, grid_loss_sys_cor_master_data
    )

    assert result_df.count() == 3
    assert (
        result_df.filter(col(Colname.energy_supplier_id) == "A")
        .filter(col(f"{Colname.time_window_start}") == time_window_1["start"])
        .collect()[0][Colname.sum_quantity]
        == default_sum_quantity + gagl_result_1
    )
    assert (
        result_df.filter(col(Colname.energy_supplier_id) == "B")
        .filter(col(f"{Colname.time_window_start}") == time_window_2["start"])
        .collect()[0][Colname.sum_quantity]
        == default_sum_quantity
    )
    assert (
        result_df.filter(col(Colname.energy_supplier_id) == "B")
        .filter(col(f"{Colname.time_window_start}") == time_window_3["start"])
        .collect()[0][Colname.sum_quantity]
        == default_sum_quantity + gagl_result_3
    )


def test_that_the_correct_metering_point_type_is_put_on_the_result(
    flex_consumption_result_row_factory: Callable[..., DataFrame],
    positive_grid_loss_result_row_factory: Callable[..., DataFrame],
    grid_loss_sys_cor_row_factory: Callable[..., DataFrame],
) -> None:
    flex_consumption = flex_consumption_result_row_factory(supplier="A")
    positive_grid_loss = positive_grid_loss_result_row_factory()

    grid_loss_sys_cor_master_data = grid_loss_sys_cor_row_factory(supplier="A")

    result_df = adjust_flex_consumption(
        flex_consumption, positive_grid_loss, grid_loss_sys_cor_master_data
    )

    assert (
        result_df.filter(col(Colname.energy_supplier_id) == "A").collect()[0][
            Colname.metering_point_type
        ]
        == MeteringPointType.CONSUMPTION.value
    )

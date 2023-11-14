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
    TimestampType,
    BooleanType,
    DecimalType,
    ArrayType,
)

from package.calculation.energy.energy_results import (
    EnergyResults,
    energy_results_schema,
)
from package.calculation.energy.transformations import adjust_production
from package.codelists import (
    MeteringPointType,
    QuantityQuality,
)
from package.constants import Colname

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


@pytest.fixture(scope="module")
def negative_grid_loss_result_schema() -> StructType:
    """
    Input system correction result schema
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
        .add(Colname.qualities, ArrayType(StringType(), False), False)
        .add(Colname.metering_point_type, StringType())
    )


@pytest.fixture(scope="module")
def sys_cor_schema() -> StructType:
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
def hourly_production_result_row_factory(
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
def negative_grid_loss_result_row_factory(
    hourly_production_result_row_factory: Callable[..., EnergyResults],
) -> Callable[..., EnergyResults]:
    """
    Factory to generate a single row of  data, with default parameters as specified above.
    """

    def factory(
        domain: str = default_domain,
        time_window=None,
        negative_grid_loss: Decimal = default_negative_grid_loss,
        aggregated_quality: str = default_aggregated_quality,
    ) -> EnergyResults:
        if time_window is None:
            time_window = default_time_window
        return hourly_production_result_row_factory(
            domain=domain,
            time_window=time_window,
            sum_quantity=negative_grid_loss,
            aggregated_quality=aggregated_quality,
        )

    return factory


@pytest.fixture(scope="module")
def sys_cor_row_factory(
    spark: SparkSession, sys_cor_schema: StructType
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


def test_grid_area_negative_grid_loss_is_added_to_grid_loss_responsible_energy_supplier(
    hourly_production_result_row_factory: Callable[..., EnergyResults],
    negative_grid_loss_result_row_factory: Callable[..., EnergyResults],
    sys_cor_row_factory: Callable[..., DataFrame],
) -> None:
    production = hourly_production_result_row_factory(supplier="A")
    negative_grid_loss = negative_grid_loss_result_row_factory()
    grid_loss_sys_cor_master_data = sys_cor_row_factory(supplier="A")

    result_df = adjust_production(
        production, negative_grid_loss, grid_loss_sys_cor_master_data
    )

    assert (
        result_df.df.where(col(Colname.energy_supplier_id) == "A").collect()[0][
            Colname.sum_quantity
        ]
        == default_negative_grid_loss + default_sum_quantity
    )


def test_grid_area_grid_loss_is_not_added_to_non_grid_loss_energy_responsible(
    hourly_production_result_row_factory: Callable[..., EnergyResults],
    negative_grid_loss_result_row_factory: Callable[..., EnergyResults],
    sys_cor_row_factory: Callable[..., DataFrame],
) -> None:
    production = hourly_production_result_row_factory(supplier="A")
    negative_grid_loss = negative_grid_loss_result_row_factory()
    grid_loss_sys_cor_master_data = sys_cor_row_factory(supplier="B")

    result_df = adjust_production(
        production, negative_grid_loss, grid_loss_sys_cor_master_data
    )

    assert (
        result_df.df.where(col(Colname.energy_supplier_id) == "A").collect()[0][
            Colname.sum_quantity
        ]
        == default_sum_quantity
    )


def test_result_dataframe_contains_same_number_of_results_with_same_energy_suppliers_as_flex_consumption_result_dataframe(
    hourly_production_result_row_factory: Callable[..., EnergyResults],
    negative_grid_loss_result_row_factory: Callable[..., EnergyResults],
    sys_cor_row_factory: Callable[..., DataFrame],
) -> None:
    hp_row_1 = hourly_production_result_row_factory(supplier="A")
    hp_row_2 = hourly_production_result_row_factory(supplier="B")
    hp_row_3 = hourly_production_result_row_factory(supplier="C")

    production = EnergyResults(hp_row_1.df.union(hp_row_2.df).union(hp_row_3.df))
    negative_grid_loss = negative_grid_loss_result_row_factory()
    grid_loss_sys_cor_master_data = sys_cor_row_factory(supplier="C")

    result_df = adjust_production(
        production, negative_grid_loss, grid_loss_sys_cor_master_data
    )

    result_df_collect = result_df.df.collect()
    assert result_df.df.count() == 3
    assert result_df_collect[0][Colname.energy_supplier_id] == "A"
    assert result_df_collect[1][Colname.energy_supplier_id] == "B"
    assert result_df_collect[2][Colname.energy_supplier_id] == "C"


def test_correct_negative_grid_loss_entry_is_used_to_determine_energy_responsible_for_the_given_time_window_from_hourly_production_result_dataframe(
    hourly_production_result_row_factory: Callable[..., EnergyResults],
    negative_grid_loss_result_row_factory: Callable[..., EnergyResults],
    sys_cor_row_factory: Callable[..., DataFrame],
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

    hp_row_1 = hourly_production_result_row_factory(
        supplier="A", time_window=time_window_1
    )
    hp_row_2 = hourly_production_result_row_factory(
        supplier="B", time_window=time_window_2
    )
    hp_row_3 = hourly_production_result_row_factory(
        supplier="B", time_window=time_window_3
    )

    production = EnergyResults(hp_row_1.df.union(hp_row_2.df).union(hp_row_3.df))

    gasc_result_1 = Decimal(1)
    gasc_result_2 = Decimal(2)
    gasc_result_3 = Decimal(3)

    gasc_row_1 = negative_grid_loss_result_row_factory(
        time_window=time_window_1, negative_grid_loss=gasc_result_1
    )
    gasc_row_2 = negative_grid_loss_result_row_factory(
        time_window=time_window_2, negative_grid_loss=gasc_result_2
    )
    gasc_row_3 = negative_grid_loss_result_row_factory(
        time_window=time_window_3, negative_grid_loss=gasc_result_3
    )

    negative_grid_loss = EnergyResults(
        gasc_row_1.df.union(gasc_row_2.df).union(gasc_row_3.df)
    )

    sc_row_1 = sys_cor_row_factory(
        supplier="A", valid_from=time_window_1["start"], valid_to=time_window_1["end"]
    )
    sc_row_2 = sys_cor_row_factory(
        supplier="C", valid_from=time_window_2["start"], valid_to=time_window_2["end"]
    )
    sc_row_3 = sys_cor_row_factory(
        supplier="B", valid_from=time_window_3["start"], valid_to=None
    )

    grid_loss_sys_cor_master_data = sc_row_1.union(sc_row_2).union(sc_row_3)

    result_df = adjust_production(
        production, negative_grid_loss, grid_loss_sys_cor_master_data
    )

    assert result_df.df.count() == 3
    assert (
        result_df.df.where(col(Colname.energy_supplier_id) == "A")
        .filter(col(f"{Colname.time_window_start}") == time_window_1["start"])
        .collect()[0][Colname.sum_quantity]
        == default_sum_quantity + gasc_result_1
    )
    assert (
        result_df.df.where(col(Colname.energy_supplier_id) == "B")
        .filter(col(f"{Colname.time_window_start}") == time_window_2["start"])
        .collect()[0][Colname.sum_quantity]
        == default_sum_quantity
    )
    assert (
        result_df.df.where(col(Colname.energy_supplier_id) == "B")
        .filter(col(f"{Colname.time_window_start}") == time_window_3["start"])
        .collect()[0][Colname.sum_quantity]
        == default_sum_quantity + gasc_result_3
    )

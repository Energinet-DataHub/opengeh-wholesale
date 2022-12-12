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
from geh_stream.codelists import Colname, ResultKeyName, ResolutionDuration
from geh_stream.aggregation_utils.aggregators import combine_added_system_correction_with_master_data, combine_added_grid_loss_with_master_data
from geh_stream.shared.data_classes import Metadata
from tests.helpers.dataframe_creators import aggregation_result_factory
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType, BooleanType
from unittest.mock import Mock
import pytest
import pandas as pd


@pytest.fixture(scope="module")
def grid_loss_sys_cor_master_data_result_schema():
    """
    Input grid loss system correction master result schema
    """
    return StructType() \
        .add(Colname.metering_point_id, StringType()) \
        .add(Colname.from_date, TimestampType()) \
        .add(Colname.to_date, TimestampType()) \
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.energy_supplier_id, StringType()) \
        .add(Colname.is_grid_loss, BooleanType()) \
        .add(Colname.is_system_correction, BooleanType())


@pytest.fixture(scope="module")
def grid_loss_sys_cor_master_data_result_factory(spark, grid_loss_sys_cor_master_data_result_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Colname.metering_point_id: ["578710000000000000", "578710000000000000"],
            Colname.from_date: [datetime(2018, 12, 31, 23, 0), datetime(2019, 12, 31, 23, 0)],
            Colname.to_date: [datetime(2019, 12, 31, 23, 0), datetime(2020, 12, 31, 23, 0)],
            Colname.grid_area: ["500", "500"],
            Colname.energy_supplier_id: ["8100000000115", "8100000000115"],
            Colname.is_grid_loss: [True, False],
            Colname.is_system_correction: [False, True],
        })

        return spark.createDataFrame(pandas_df, schema=grid_loss_sys_cor_master_data_result_schema)
    return factory


@pytest.fixture(scope="module")
def expected_combined_data_schema():
    """
    Input grid loss system correction master result schema
    """
    return StructType() \
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.quantity, DecimalType()) \
        .add(Colname.time_window, StructType()
             .add(Colname.start, TimestampType())
             .add(Colname.end, TimestampType()),
             False) \
        .add(Colname.metering_point_id, StringType()) \
        .add(Colname.from_date, TimestampType()) \
        .add(Colname.to_date, TimestampType()) \
        .add(Colname.resolution, StringType()) \
        .add(Colname.energy_supplier_id, StringType()) \
        .add(Colname.balance_responsible_id, StringType()) \
        .add(Colname.in_grid_area, StringType()) \
        .add(Colname.out_grid_area, StringType()) \
        .add(Colname.metering_point_type, StringType()) \
        .add(Colname.settlement_method, StringType()) \
        .add(Colname.is_grid_loss, BooleanType()) \
        .add(Colname.is_system_correction, BooleanType())


@pytest.fixture(scope="module")
def expected_combined_data_factory(spark, expected_combined_data_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Colname.grid_area: ["500", "500"],
            Colname.added_grid_loss: [Decimal(6.0), Decimal(6.0)],
            Colname.time_window: [
                {Colname.start: datetime(2019, 1, 1, 0, 0), Colname.end: datetime(2019, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)}
            ],
            Colname.metering_point_id: ["578710000000000000", "578710000000000000"],
            Colname.from_date: [datetime(2018, 12, 31, 23, 0), datetime(2019, 12, 31, 23, 0)],
            Colname.to_date: [datetime(2019, 12, 31, 23, 0), datetime(2020, 12, 31, 23, 0)],
            Colname.resolution: ["PT1H", "PT1H"],
            Colname.energy_supplier_id: ["8100000000115", "8100000000115"],
            Colname.balance_responsible_id: ["8100000000214", "8100000000214"],
            Colname.in_grid_area: [None, None],
            Colname.out_grid_area: [None, None],
            Colname.metering_point_type: ["E17", "E17"],
            Colname.settlement_method: ["D01", "D01"],
            Colname.is_grid_loss: [True, False],
            Colname.is_system_correction: [False, True],
        })

        return spark.createDataFrame(pandas_df, schema=expected_combined_data_schema)
    return factory


def test_combine_added_system_correction_with_master_data(grid_loss_sys_cor_master_data_result_factory, aggregation_result_factory, expected_combined_data_factory):
    metadata = Metadata("1", "1", "1", "1", "1")
    results = {}
    results[ResultKeyName.grid_loss_sys_cor_master_data] = grid_loss_sys_cor_master_data_result_factory()
    added_sys_cor_1 = aggregation_result_factory(
        grid_area="500",
        added_system_correction=Decimal(6.0),
        time_window_start=datetime(2019, 1, 1, 0, 0),
        time_window_end=datetime(2019, 1, 1, 1, 0),
        resolution=ResolutionDuration.hour,
        energy_supplier_id="8100000000115",
        balance_responsible_id="8100000000214",
        settlement_method="D01"
    )
    added_sys_cor_2 = aggregation_result_factory(
        grid_area="500",
        added_system_correction=Decimal(6.0),
        time_window_start=datetime(2020, 1, 1, 0, 0),
        time_window_end=datetime(2020, 1, 1, 1, 0),
        resolution=ResolutionDuration.hour,
        energy_supplier_id="8100000000115",
        balance_responsible_id="8100000000214",
        settlement_method="D01"
    )
    results[ResultKeyName.added_system_correction] = added_sys_cor_1.union(added_sys_cor_2)
    expected_combined_data_factory = expected_combined_data_factory()

    result = combine_added_system_correction_with_master_data(results, metadata)

    # expected data for combine_added_grid_loss_with_master_data is at index 1 in expected_combined_data_factory
    assert result.collect()[0] == expected_combined_data_factory.collect()[1]


def test_combine_added_grid_loss_with_master_data(grid_loss_sys_cor_master_data_result_factory, aggregation_result_factory, expected_combined_data_factory):
    metadata = Metadata("1", "1", "1", "1", "1")
    results = {}
    results[ResultKeyName.grid_loss_sys_cor_master_data] = grid_loss_sys_cor_master_data_result_factory()
    added_grid_loss_1 = aggregation_result_factory(
        grid_area="500",
        added_grid_loss=Decimal(6.0),
        time_window_start=datetime(2019, 1, 1, 0, 0),
        time_window_end=datetime(2019, 1, 1, 1, 0),
        resolution=ResolutionDuration.hour,
        energy_supplier_id="8100000000115",
        balance_responsible_id="8100000000214",
        settlement_method="D01"
    )
    added_grid_loss_2 = aggregation_result_factory(
        grid_area="500",
        added_grid_loss=Decimal(6.0),
        time_window_start=datetime(2020, 1, 1, 0, 0),
        time_window_end=datetime(2020, 1, 1, 1, 0),
        resolution=ResolutionDuration.hour,
        energy_supplier_id="8100000000115",
        balance_responsible_id="8100000000214",
        settlement_method="D01"
    )
    results[ResultKeyName.added_grid_loss] = added_grid_loss_1.union(added_grid_loss_2)
    expected_combined_data_factory = expected_combined_data_factory()

    result = combine_added_grid_loss_with_master_data(results, metadata)

    # expected data for combine_added_grid_loss_with_master_data is at index 0 in expected_combined_data_factory
    assert result.collect()[0] == expected_combined_data_factory.collect()[0]

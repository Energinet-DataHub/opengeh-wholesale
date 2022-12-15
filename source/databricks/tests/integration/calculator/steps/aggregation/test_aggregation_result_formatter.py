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
    TimeSeriesQuality,
)
from package.steps.aggregation.aggregation_result_formatter import (
    create_dataframe_from_aggregation_result_schema,
)
from package.shared.data_classes import Metadata
from package.schemas.output import aggregation_result_schema
import pytest
import pandas as pd
from tests.helpers import DataframeDefaults
from package.constants import Colname


@pytest.fixture(scope="module")
def aggregation_result_factory(spark):
    def factory(
        job_id=DataframeDefaults.default_job_id,
        snapshot_id=DataframeDefaults.default_snapshot_id,
        result_id=DataframeDefaults.default_result_id,
        result_name=DataframeDefaults.default_result_name,
        result_path=DataframeDefaults.default_result_path,
        grid_area=DataframeDefaults.default_grid_area,
        in_grid_area=None,
        out_grid_area=None,
        balance_responsible_id=None,
        energy_supplier_id=None,
        time_window_start=DataframeDefaults.default_time_window_start,
        time_window_end=DataframeDefaults.default_time_window_end,
        resolution=DataframeDefaults.default_metering_point_resolution,
        sum_quantity=DataframeDefaults.default_sum_quantity,
        quality=DataframeDefaults.default_quality,
        metering_point_type=DataframeDefaults.default_metering_point_type,
        settlement_method=None,
        added_grid_loss=None,
        added_system_correction=None,
        position=None,
    ):
        pandas_df = pd.DataFrame().append(
            [
                {
                    Colname.job_id: job_id,
                    Colname.snapshot_id: snapshot_id,
                    Colname.result_id: result_id,
                    Colname.result_name: result_name,
                    Colname.result_path: result_path,
                    Colname.grid_area: grid_area,
                    Colname.in_grid_area: in_grid_area,
                    Colname.out_grid_area: out_grid_area,
                    Colname.balance_responsible_id: balance_responsible_id,
                    Colname.energy_supplier_id: energy_supplier_id,
                    Colname.time_window: {
                        Colname.start: time_window_start,
                        Colname.end: time_window_end,
                    },
                    Colname.resolution: resolution,
                    Colname.sum_quantity: sum_quantity,
                    Colname.quality: quality,
                    Colname.metering_point_type: metering_point_type,
                    Colname.settlement_method: settlement_method,
                    Colname.added_grid_loss: added_grid_loss,
                    Colname.added_system_correction: added_system_correction,
                    Colname.position: position,
                }
            ],
            ignore_index=True,
        )

        return spark.createDataFrame(pandas_df, schema=aggregation_result_schema)

    return factory


@pytest.fixture(scope="module")
def agg_result_factory(spark):
    def factory(
        grid_area="A",
        start=datetime(2020, 1, 1, 0, 0),
        end=datetime(2020, 1, 1, 1, 0),
        resolution=MeteringPointResolution.hour.value,
        sum_quantity=Decimal("1.234"),
        quality=TimeSeriesQuality.estimated.value,
        metering_point_type=MeteringPointType.consumption.value,
    ):
        return spark.createDataFrame(
            pd.DataFrame().append(
                [
                    {
                        Colname.grid_area: grid_area,
                        Colname.time_window: {Colname.start: start, Colname.end: end},
                        Colname.resolution: resolution,
                        Colname.sum_quantity: sum_quantity,
                        Colname.quality: quality,
                        Colname.metering_point_type: metering_point_type,
                    }
                ],
                ignore_index=True,
            )
        )

    return factory


def test__create_dataframe_from_aggregation_result_schema__can_create_a_dataframe_that_match_aggregation_result_schema(
    agg_result_factory,
):
    # Arrange
    metadata = Metadata("1", "1", "1", "1", "1")
    result = agg_result_factory()
    # Act
    actual = create_dataframe_from_aggregation_result_schema(metadata, result)
    # Assert
    assert actual.schema == aggregation_result_schema


def test__create_dataframe_from_aggregation_result_schema__match_expected_dataframe(
    agg_result_factory, aggregation_result_factory
):
    # Arrange
    metadata = Metadata("1", "1", "1", "1", "1")
    result = agg_result_factory()
    expected = aggregation_result_factory(
        grid_area="A",
        time_window_start=datetime(2020, 1, 1, 0, 0),
        time_window_end=datetime(2020, 1, 1, 1, 0),
        resolution=MeteringPointResolution.hour.value,
        sum_quantity=Decimal("1.234"),
        quality=TimeSeriesQuality.estimated.value,
        metering_point_type=MeteringPointType.consumption.value,
    )
    # Act
    actual = create_dataframe_from_aggregation_result_schema(metadata, result)
    # Assert
    assert actual.collect() == expected.collect()

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
from datetime import datetime, timedelta
from geh_stream.codelists import Colname, ResultKeyName, ResolutionDuration, MarketEvaluationPointType
from geh_stream.aggregation_utils.aggregators import \
    aggregate_hourly_production_ga_es, \
    aggregate_hourly_production_ga_brp, \
    aggregate_hourly_production_ga
from geh_stream.shared.data_classes import Metadata
from geh_stream.aggregation_utils.aggregation_result_formatter import create_dataframe_from_aggregation_result_schema
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
import pytest
import pandas as pd
from geh_stream.codelists import Quality

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime("2020-01-01T00:00:00+0000", date_time_formatting_string)

metadata = Metadata("1", "1", "1", "1", "1")


@pytest.fixture(scope="module")
def agg_production_schema():
    return StructType() \
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.balance_responsible_id, StringType()) \
        .add(Colname.energy_supplier_id, StringType()) \
        .add(Colname.time_window,
             StructType()
             .add(Colname.start, TimestampType())
             .add(Colname.end, TimestampType()),
             False) \
        .add(Colname.sum_quantity, DecimalType(20)) \
        .add(Colname.quality, StringType()) \
        .add(Colname.resolution, StringType()) \
        .add(Colname.metering_point_type, StringType())


@pytest.fixture(scope="module")
def test_data_factory(spark, agg_production_schema):

    def factory():
        pandas_df = pd.DataFrame({
            Colname.grid_area: [],
            Colname.balance_responsible_id: [],
            Colname.energy_supplier_id: [],
            Colname.time_window: [],
            Colname.sum_quantity: [],
            Colname.quality: [],
            Colname.resolution: [],
            Colname.metering_point_type: []
        })
        for i in range(3):
            for j in range(5):
                for k in range(10):
                    pandas_df = pandas_df.append({
                        Colname.grid_area: str(i),
                        Colname.balance_responsible_id: str(j),
                        Colname.energy_supplier_id: str(k),
                        Colname.time_window: {
                            Colname.start: default_obs_time + timedelta(hours=i),
                            Colname.end: default_obs_time + timedelta(hours=i + 1)},
                        Colname.sum_quantity: Decimal(i + j + k),
                        Colname.quality: [Quality.estimated.value],
                        Colname.resolution: [ResolutionDuration.hour],
                        Colname.metering_point_type: [MarketEvaluationPointType.production.value]
                    }, ignore_index=True)
        return spark.createDataFrame(pandas_df, schema=agg_production_schema)
    return factory


def test_production_calculation_per_ga_and_es(test_data_factory):
    results = {}
    results[ResultKeyName.hourly_production_with_system_correction_and_grid_loss] = create_dataframe_from_aggregation_result_schema(metadata, test_data_factory())
    result = aggregate_hourly_production_ga_es(results, metadata).sort(Colname.grid_area, Colname.energy_supplier_id)
    result_collect = result.collect()
    assert result_collect[0][Colname.balance_responsible_id] is None
    assert result_collect[0][Colname.grid_area] == "0"
    assert result_collect[9][Colname.energy_supplier_id] == "9"
    assert result_collect[10][Colname.sum_quantity] == Decimal("15")
    assert result_collect[29][Colname.grid_area] == "2"
    assert result_collect[29][Colname.energy_supplier_id] == "9"
    assert result_collect[29][Colname.sum_quantity] == Decimal("65")


def test_production_calculation_per_ga_and_brp(test_data_factory):
    results = {}
    results[ResultKeyName.hourly_production_with_system_correction_and_grid_loss] = create_dataframe_from_aggregation_result_schema(metadata, test_data_factory())
    result = aggregate_hourly_production_ga_brp(results, metadata).sort(Colname.grid_area, Colname.balance_responsible_id)
    result_collect = result.collect()
    assert result_collect[0][Colname.energy_supplier_id] is None
    assert result_collect[0][Colname.sum_quantity] == Decimal("45")
    assert result_collect[4][Colname.grid_area] == "0"
    assert result_collect[5][Colname.balance_responsible_id] == "0"
    assert result_collect[14][Colname.grid_area] == "2"
    assert result_collect[14][Colname.balance_responsible_id] == "4"
    assert result_collect[14][Colname.sum_quantity] == Decimal("105")


def test_production_calculation_per_ga(test_data_factory):
    results = {}
    results[ResultKeyName.hourly_production_with_system_correction_and_grid_loss] = create_dataframe_from_aggregation_result_schema(metadata, test_data_factory())
    result = aggregate_hourly_production_ga(results, metadata).sort(Colname.grid_area)
    result_collect = result.collect()
    assert result_collect[0][Colname.balance_responsible_id] is None
    assert result_collect[0][Colname.energy_supplier_id] is None
    assert result_collect[0][Colname.grid_area] == "0"
    assert result_collect[1][Colname.sum_quantity] == Decimal("375")
    assert result_collect[2][Colname.grid_area] == "2"
    assert result_collect[2][Colname.sum_quantity] == Decimal("425")

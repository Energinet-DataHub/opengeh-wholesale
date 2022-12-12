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

from numpy import append
from geh_stream.codelists import Colname, Quality, ResultKeyName, ResolutionDuration, MarketEvaluationPointType
from geh_stream.aggregation_utils.aggregators import calculate_total_consumption
from geh_stream.shared.data_classes import Metadata
from geh_stream.aggregation_utils.aggregation_result_formatter import create_dataframe_from_aggregation_result_schema
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
import pytest
import pandas as pd


metadata = Metadata("1", "1", "1", "1", "1")


@pytest.fixture(scope="module")
def net_exchange_schema():
    return StructType() \
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.time_window,
             StructType()
             .add(Colname.start, TimestampType())
             .add(Colname.end, TimestampType()),
             False) \
        .add("in_sum", DecimalType(20, 1)) \
        .add("out_sum", DecimalType(20, 1)) \
        .add(Colname.sum_quantity, DecimalType(20, 1)) \
        .add(Colname.quality, StringType()) \
        .add(Colname.resolution, StringType()) \
        .add(Colname.metering_point_type, StringType())


@pytest.fixture(scope="module")
def agg_net_exchange_factory(spark, net_exchange_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Colname.grid_area: ["1", "1", "1", "1", "1", "2"],
            Colname.time_window: [
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 1, 0), Colname.end: datetime(2020, 1, 1, 2, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)}
            ],
            "in_sum": [Decimal(2.0), Decimal(2.0), Decimal(2.0), Decimal(2.0), Decimal(2.0), Decimal(2.0)],
            "out_sum": [Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0)],
            Colname.sum_quantity: [Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0)],
            Colname.quality: ["56", "56", "56", "56", "QM", "56"],
            Colname.resolution: [ResolutionDuration.hour, ResolutionDuration.hour, ResolutionDuration.hour, ResolutionDuration.hour,
                                 ResolutionDuration.hour, ResolutionDuration.hour],
            Colname.metering_point_type: [MarketEvaluationPointType.consumption.value, MarketEvaluationPointType.consumption.value,
                                          MarketEvaluationPointType.consumption.value, MarketEvaluationPointType.consumption.value,
                                          MarketEvaluationPointType.consumption.value, MarketEvaluationPointType.consumption.value]
        })

        return spark.createDataFrame(pandas_df, schema=net_exchange_schema)
    return factory


@pytest.fixture(scope="module")
def production_schema():
    return StructType() \
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.time_window,
             StructType()
             .add(Colname.start, TimestampType())
             .add(Colname.end, TimestampType()),
             False) \
        .add(Colname.sum_quantity, DecimalType(20, 1)) \
        .add(Colname.quality, StringType()) \
        .add(Colname.resolution, StringType()) \
        .add(Colname.metering_point_type, StringType())


@pytest.fixture(scope="module")
def agg_production_factory(spark, production_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Colname.grid_area: ["1", "1", "1", "1", "1", "2"],
            Colname.time_window: [
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 1, 0), Colname.end: datetime(2020, 1, 1, 2, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)}
            ],
            Colname.sum_quantity: [Decimal(1.0), Decimal(2.0), Decimal(3.0), Decimal(4.0), Decimal(5.0), Decimal(6.0)],
            Colname.quality: ["56", "56", "56", "56", "E01", "56"],
            Colname.resolution: [ResolutionDuration.hour, ResolutionDuration.hour, ResolutionDuration.hour, ResolutionDuration.hour,
                                 ResolutionDuration.hour, ResolutionDuration.hour],
            Colname.metering_point_type: [MarketEvaluationPointType.production.value, MarketEvaluationPointType.production.value,
                                          MarketEvaluationPointType.production.value, MarketEvaluationPointType.production.value,
                                          MarketEvaluationPointType.production.value, MarketEvaluationPointType.production.value]
        })

        return spark.createDataFrame(pandas_df, schema=production_schema)
    return factory


@pytest.fixture(scope="module")
def agg_total_production_factory(spark, production_schema):
    def factory(quality):
        pandas_df = pd.DataFrame({
            Colname.grid_area: [],
            Colname.time_window: [],
            Colname.sum_quantity: [],
            Colname.quality: [],
            Colname.resolution: [],
            Colname.metering_point_type: []})

        pandas_df = pandas_df.append({
            Colname.grid_area: "1",
            Colname.time_window: {
                Colname.start: datetime(2020, 1, 1, 0, 0),
                Colname.end: datetime(2020, 1, 1, 1, 0)
                           },
            Colname.sum_quantity: Decimal(1.0),
            Colname.quality: quality,
            Colname.resolution: [ResolutionDuration.hour],
            Colname.metering_point_type: [MarketEvaluationPointType.production.value]
        }, ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=production_schema)
    return factory


@pytest.fixture(scope="module")
def agg_total_net_exchange_factory(spark, net_exchange_schema):
    def factory(quality):
        pandas_df = pd.DataFrame({
            Colname.grid_area: [],
            Colname.time_window: [],
            "in_sum": [],
            "out_sum": [],
            Colname.sum_quantity: [],
            Colname.quality: [],
            Colname.resolution: [],
            Colname.metering_point_type: []
        })

        pandas_df = pandas_df.append({
            Colname.grid_area: "1",
            Colname.time_window: {
                Colname.start: datetime(2020, 1, 1, 0, 0),
                Colname.end: datetime(2020, 1, 1, 1, 0)
                },
            "in_sum": Decimal(1.0),
            "out_sum": Decimal(1.0),
            Colname.sum_quantity: Decimal(1.0),
            Colname.quality: quality,
            Colname.resolution: [ResolutionDuration.hour],
            Colname.metering_point_type: [MarketEvaluationPointType.exchange.value]
        }, ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=net_exchange_schema)
    return factory


def test_grid_area_total_consumption(agg_net_exchange_factory, agg_production_factory):
    results = {}
    results[ResultKeyName.net_exchange_per_ga] = create_dataframe_from_aggregation_result_schema(metadata, agg_net_exchange_factory())
    results[ResultKeyName.hourly_production_ga] = create_dataframe_from_aggregation_result_schema(metadata, agg_production_factory())
    aggregated_df = calculate_total_consumption(results, metadata)
    aggregated_df_collect = aggregated_df.collect()
    assert aggregated_df_collect[0][Colname.sum_quantity] == Decimal("14.0") and \
        aggregated_df_collect[1][Colname.sum_quantity] == Decimal("6.0") and \
        aggregated_df_collect[2][Colname.sum_quantity] == Decimal("7.0")


@pytest.mark.parametrize("prod_quality, ex_quality, expected_quality", [
                        (Quality.estimated.value, Quality.estimated.value, Quality.estimated.value),
                        (Quality.estimated.value, Quality.quantity_missing.value, Quality.estimated.value),
                        (Quality.estimated.value, Quality.as_read.value, Quality.estimated.value),
                        (Quality.quantity_missing.value, Quality.quantity_missing.value, Quality.estimated.value),
                        (Quality.quantity_missing.value, Quality.as_read.value, Quality.estimated.value),
                        (Quality.as_read.value, Quality.as_read.value, Quality.as_read.value)
                        ])
def test_aggregated_quality(
    agg_total_production_factory,
    agg_total_net_exchange_factory,
    prod_quality, ex_quality,
    expected_quality
                            ):

    results = {}
    results[ResultKeyName.net_exchange_per_ga] = create_dataframe_from_aggregation_result_schema(metadata, agg_total_net_exchange_factory(ex_quality))
    results[ResultKeyName.hourly_production_ga] = create_dataframe_from_aggregation_result_schema(metadata, agg_total_production_factory(prod_quality))

    result_df = calculate_total_consumption(results, metadata)

    assert result_df.collect()[0][Colname.quality] == expected_quality

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
from enum import Enum
from geh_stream.codelists import Colname, ResultKeyName, MarketEvaluationPointType, ResolutionDuration, SettlementMethod
from geh_stream.aggregation_utils.aggregators import calculate_grid_loss, calculate_residual_ga
from geh_stream.codelists import Quality
from geh_stream.shared.data_classes import Metadata
from geh_stream.aggregation_utils.aggregation_result_formatter import create_dataframe_from_aggregation_result_schema
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
from pyspark.sql.functions import col
import pytest
import pandas as pd


date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime("2020-01-01T00:00:00+0000", date_time_formatting_string)

metadata = Metadata("1", "1", "1", "1", "1")


class AggregationMethod(Enum):
    net_exchange = "net_exchange"
    hourly_consumption = "hourly_consumption"
    flex_consumption = "flex_consumption"
    production = "production"


@pytest.fixture(scope="module")
def agg_net_exchange_schema():
    return StructType() \
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.time_window,
             StructType()
             .add(Colname.start, TimestampType())
             .add(Colname.end, TimestampType()),
             False) \
        .add(Colname.sum_quantity, DecimalType(38)) \
        .add(Colname.quality, StringType()) \
        .add(Colname.resolution, StringType()) \
        .add(Colname.metering_point_type, StringType())


@pytest.fixture(scope="module")
def agg_consumption_and_production_schema():
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
def agg_result_factory(spark, agg_net_exchange_schema, agg_consumption_and_production_schema):
    """
    Factory to generate a single row of time series data, with default parameters as specified above.
    """
    def factory(agg_method: AggregationMethod):
        if agg_method == AggregationMethod.net_exchange:
            pandas_df = pd.DataFrame({
                Colname.grid_area: [],
                Colname.time_window: [],
                Colname.sum_quantity: [],
                Colname.quality: [],
                Colname.resolution: [],
                Colname.metering_point_type: []
            })
            for i in range(10):
                pandas_df = pandas_df.append({
                    Colname.grid_area: str(i),
                    Colname.time_window: {Colname.start: default_obs_time + timedelta(hours=i), Colname.end: default_obs_time + timedelta(hours=i + 1)},
                    Colname.sum_quantity: Decimal(20 + i),
                    Colname.quality: Quality.estimated.value,
                    Colname.resolution: ResolutionDuration.hour,
                    Colname.metering_point_type: MarketEvaluationPointType.exchange.value
                }, ignore_index=True)
            return spark.createDataFrame(pandas_df, schema=agg_net_exchange_schema)
        elif agg_method == AggregationMethod.hourly_consumption:
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
            for i in range(10):
                pandas_df = pandas_df.append({
                    Colname.grid_area: str(i),
                    Colname.balance_responsible_id: str(i),
                    Colname.energy_supplier_id: str(i),
                    Colname.time_window: {Colname.start: default_obs_time + timedelta(hours=i), Colname.end: default_obs_time + timedelta(hours=i + 1)},
                    Colname.sum_quantity: Decimal(13 + i),
                    Colname.quality: Quality.estimated.value,
                    Colname.resolution: ResolutionDuration.hour,
                    Colname.metering_point_type: MarketEvaluationPointType.consumption.value
                }, ignore_index=True)
            return spark.createDataFrame(pandas_df, schema=agg_consumption_and_production_schema)
        elif agg_method == AggregationMethod.flex_consumption:
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
            for i in range(10):
                pandas_df = pandas_df.append({
                    Colname.grid_area: str(i),
                    Colname.balance_responsible_id: str(i),
                    Colname.energy_supplier_id: str(i),
                    Colname.time_window: {Colname.start: default_obs_time + timedelta(hours=i), Colname.end: default_obs_time + timedelta(hours=i + 1)},
                    Colname.sum_quantity: Decimal(14 + i),
                    Colname.quality: Quality.estimated.value,
                    Colname.resolution: ResolutionDuration.hour,
                    Colname.metering_point_type: MarketEvaluationPointType.consumption.value
                }, ignore_index=True)
            return spark.createDataFrame(pandas_df, schema=agg_consumption_and_production_schema)
        elif agg_method == AggregationMethod.production:
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
            for i in range(10):
                pandas_df = pandas_df.append({
                    Colname.grid_area: str(i),
                    Colname.balance_responsible_id: str(i),
                    Colname.energy_supplier_id: str(i),
                    Colname.time_window: {Colname.start: default_obs_time + timedelta(hours=i), Colname.end: default_obs_time + timedelta(hours=i + 1)},
                    Colname.sum_quantity: Decimal(50 + i),
                    Colname.quality: Quality.estimated.value,
                    Colname.resolution: ResolutionDuration.hour,
                    Colname.metering_point_type: MarketEvaluationPointType.production.value
                }, ignore_index=True)
            return spark.createDataFrame(pandas_df, schema=agg_consumption_and_production_schema)
    return factory


@pytest.fixture(scope="module")
def agg_net_exchange_factory(spark, agg_net_exchange_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Colname.grid_area: ["1", "1", "1", "2", "2", "3"],
            Colname.time_window: [
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 1, 0), Colname.end: datetime(2020, 1, 1, 2, 0)},
                {Colname.start: datetime(2020, 1, 1, 2, 0), Colname.end: datetime(2020, 1, 1, 3, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 1, 0), Colname.end: datetime(2020, 1, 1, 2, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)}
            ],
            Colname.sum_quantity: [Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0), Decimal(1.0)],
            Colname.quality: ["56", "56", "56", "56", "56", "56"],
            Colname.resolution: [ResolutionDuration.hour, ResolutionDuration.hour, ResolutionDuration.hour, ResolutionDuration.hour,
                                 ResolutionDuration.hour, ResolutionDuration.hour],
            Colname.metering_point_type: [MarketEvaluationPointType.exchange.value, MarketEvaluationPointType.exchange.value,
                                          MarketEvaluationPointType.exchange.value, MarketEvaluationPointType.exchange.value,
                                          MarketEvaluationPointType.exchange.value, MarketEvaluationPointType.exchange.value]
        })

        return spark.createDataFrame(pandas_df, schema=agg_net_exchange_schema)
    return factory


@pytest.fixture(scope="module")
def agg_flex_consumption_factory(spark, agg_consumption_and_production_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Colname.grid_area: ["1", "1", "1", "2", "2", "3"],
            Colname.balance_responsible_id: ["1", "2", "2", "1", "2", "1"],
            Colname.energy_supplier_id: ["1", "1", "2", "1", "1", "1"],
            Colname.time_window: [
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 1, 0), Colname.end: datetime(2020, 1, 1, 2, 0)},
                {Colname.start: datetime(2020, 1, 1, 2, 0), Colname.end: datetime(2020, 1, 1, 3, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 1, 0), Colname.end: datetime(2020, 1, 1, 2, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)}
            ],
            Colname.sum_quantity: [Decimal(2.0), Decimal(6.0), Decimal(4.0), Decimal(8.0), Decimal(1.0), Decimal(2.0)],
            Colname.quality: ["56", "56", "56", "56", "56", "56"],
            Colname.resolution: [ResolutionDuration.hour, ResolutionDuration.hour, ResolutionDuration.hour, ResolutionDuration.hour,
                                 ResolutionDuration.hour, ResolutionDuration.hour],
            Colname.metering_point_type: [MarketEvaluationPointType.consumption.value, MarketEvaluationPointType.consumption.value,
                                          MarketEvaluationPointType.consumption.value, MarketEvaluationPointType.consumption.value,
                                          MarketEvaluationPointType.consumption.value, MarketEvaluationPointType.consumption.value]
        })

        return spark.createDataFrame(pandas_df, schema=agg_consumption_and_production_schema)
    return factory


@pytest.fixture(scope="module")
def agg_hourly_consumption_factory(spark, agg_consumption_and_production_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Colname.grid_area: ["1", "1", "1", "2", "2", "3"],
            Colname.balance_responsible_id: ["1", "2", "2", "1", "2", "1"],
            Colname.energy_supplier_id: ["1", "1", "2", "1", "1", "1"],
            Colname.time_window: [
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 1, 0), Colname.end: datetime(2020, 1, 1, 2, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)}
            ],
            Colname.sum_quantity: [Decimal(6.0), Decimal(1.0), Decimal(4.0), Decimal(2.0), Decimal(3.0), Decimal(1.0)],
            Colname.quality: ["56", "56", "56", "56", "56", "56"],
            Colname.resolution: [ResolutionDuration.hour, ResolutionDuration.hour, ResolutionDuration.hour, ResolutionDuration.hour,
                                 ResolutionDuration.hour, ResolutionDuration.hour],
            Colname.metering_point_type: [MarketEvaluationPointType.consumption.value, MarketEvaluationPointType.consumption.value,
                                          MarketEvaluationPointType.consumption.value, MarketEvaluationPointType.consumption.value,
                                          MarketEvaluationPointType.consumption.value, MarketEvaluationPointType.consumption.value]
        })

        return spark.createDataFrame(pandas_df, schema=agg_consumption_and_production_schema)
    return factory


@pytest.fixture(scope="module")
def agg_hourly_production_factory(spark, agg_consumption_and_production_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Colname.grid_area: ["1", "1", "1", "2", "2", "3"],
            Colname.balance_responsible_id: ["1", "2", "2", "1", "2", "1"],
            Colname.energy_supplier_id: ["1", "1", "2", "1", "1", "1"],
            Colname.time_window: [
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 1, 0), Colname.end: datetime(2020, 1, 1, 2, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)}
            ],
            Colname.sum_quantity: [Decimal(9.0), Decimal(3.0), Decimal(6.0), Decimal(3.0), Decimal(1.0), Decimal(2.0)],
            Colname.quality: ["56", "56", "56", "56", "56", "56"],
            Colname.resolution: [ResolutionDuration.hour, ResolutionDuration.hour, ResolutionDuration.hour, ResolutionDuration.hour,
                                 ResolutionDuration.hour, ResolutionDuration.hour],
            Colname.metering_point_type: [MarketEvaluationPointType.production.value, MarketEvaluationPointType.production.value,
                                          MarketEvaluationPointType.production.value, MarketEvaluationPointType.production.value,
                                          MarketEvaluationPointType.production.value, MarketEvaluationPointType.production.value]
        })

        return spark.createDataFrame(pandas_df, schema=agg_consumption_and_production_schema)
    return factory


def test_grid_loss_calculation(agg_result_factory):
    metadata = Metadata("1", "1", "1", "1", "1")
    results = {}
    results[ResultKeyName.net_exchange_per_ga] = create_dataframe_from_aggregation_result_schema(metadata, agg_result_factory(agg_method=AggregationMethod.net_exchange))
    results[ResultKeyName.hourly_consumption] = create_dataframe_from_aggregation_result_schema(metadata, agg_result_factory(agg_method=AggregationMethod.hourly_consumption))
    results[ResultKeyName.flex_consumption] = create_dataframe_from_aggregation_result_schema(metadata, agg_result_factory(agg_method=AggregationMethod.flex_consumption))
    results[ResultKeyName.hourly_production] = create_dataframe_from_aggregation_result_schema(metadata, agg_result_factory(agg_method=AggregationMethod.production))

    result = calculate_grid_loss(results, metadata)

    # Verify the calculation result is correct by checking 50+i + 20+i - (13+i + 14+i) equals 43 for all i in range 0 to 9
    assert result.filter(col(Colname.sum_quantity) != 43).count() == 0


def test_grid_loss_calculation_calculates_correctly_on_grid_area(agg_net_exchange_factory, agg_hourly_consumption_factory, agg_flex_consumption_factory, agg_hourly_production_factory):
    metadata = Metadata("1", "1", "1", "1", "1")
    results = {}
    results[ResultKeyName.net_exchange_per_ga] = create_dataframe_from_aggregation_result_schema(metadata, agg_net_exchange_factory())
    results[ResultKeyName.hourly_settled_consumption_ga] = create_dataframe_from_aggregation_result_schema(metadata, agg_hourly_consumption_factory())
    results[ResultKeyName.flex_settled_consumption_ga] = create_dataframe_from_aggregation_result_schema(metadata, agg_flex_consumption_factory())
    results[ResultKeyName.hourly_production_ga] = create_dataframe_from_aggregation_result_schema(metadata, agg_hourly_production_factory())

    result = calculate_residual_ga(results, metadata)

    result_collect = result.collect()
    assert result_collect[0][Colname.sum_quantity] == Decimal("6")
    assert result_collect[1][Colname.sum_quantity] == Decimal("0")
    assert result_collect[2][Colname.sum_quantity] == Decimal("0")
    assert result_collect[3][Colname.sum_quantity] == Decimal("-6")
    assert result_collect[4][Colname.sum_quantity] == Decimal("-2")
    assert result_collect[5][Colname.sum_quantity] == Decimal("0")

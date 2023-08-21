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
from package.codelists import (
    MeteringPointType,
    MeteringPointResolution,
    TimeSeriesQuality,
)

from package.steps.aggregation import (
    calculate_grid_loss,
)
from package.steps.aggregation.transformations import (
    create_dataframe_from_aggregation_result_schema,
)
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
from pyspark.sql.functions import col
from pyspark.sql import DataFrame, SparkSession
import pytest
import pandas as pd
from package.constants import Colname
from typing import Callable

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime(
    "2020-01-01T00:00:00+0000", date_time_formatting_string
)


class AggregationMethod(Enum):
    NET_EXCHANGE = "net_exchange"
    HOURLY_CONSUMPTION = "hourly_consumption"
    FLEX_CONSUMPTION = "flex_consumption"
    PRODUCTION = "production"


@pytest.fixture(scope="module")
def agg_net_exchange_schema() -> StructType:
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
        .add(Colname.sum_quantity, DecimalType(38))
        .add(Colname.quality, StringType())
        .add(Colname.resolution, StringType())
        .add(Colname.metering_point_type, StringType())
    )


@pytest.fixture(scope="module")
def agg_consumption_and_production_schema() -> StructType:
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
        .add(Colname.sum_quantity, DecimalType(20))
        .add(Colname.quality, StringType())
        .add(Colname.resolution, StringType())
        .add(Colname.metering_point_type, StringType())
    )


@pytest.fixture(scope="module")
def agg_result_factory(
    spark: SparkSession,
    agg_net_exchange_schema: StructType,
    agg_consumption_and_production_schema: StructType,
) -> Callable[[AggregationMethod], DataFrame]:
    """
    Factory to generate a single row of time series data, with default parameters as specified above.
    """

    def factory(agg_method: AggregationMethod) -> DataFrame:
        if agg_method == AggregationMethod.NET_EXCHANGE:
            pandas_df = pd.DataFrame(
                {
                    Colname.grid_area: [],
                    Colname.time_window: [],
                    Colname.sum_quantity: [],
                    Colname.quality: [],
                    Colname.resolution: [],
                    Colname.metering_point_type: [],
                }
            )
            for i in range(10):
                pandas_df = pandas_df.append(
                    {
                        Colname.grid_area: str(i),
                        Colname.time_window: {
                            Colname.start: default_obs_time + timedelta(hours=i),
                            Colname.end: default_obs_time + timedelta(hours=i + 1),
                        },
                        Colname.sum_quantity: Decimal(20 + i),
                        Colname.quality: TimeSeriesQuality.ESTIMATED.value,
                        Colname.resolution: MeteringPointResolution.HOUR.value,
                        Colname.metering_point_type: MeteringPointType.EXCHANGE.value,
                    },
                    ignore_index=True,
                )
            return spark.createDataFrame(pandas_df, schema=agg_net_exchange_schema)
        elif agg_method == AggregationMethod.HOURLY_CONSUMPTION:
            pandas_df = pd.DataFrame(
                {
                    Colname.grid_area: [],
                    Colname.balance_responsible_id: [],
                    Colname.energy_supplier_id: [],
                    Colname.time_window: [],
                    Colname.sum_quantity: [],
                    Colname.quality: [],
                    Colname.resolution: [],
                    Colname.metering_point_type: [],
                }
            )
            for i in range(10):
                pandas_df = pandas_df.append(
                    {
                        Colname.grid_area: str(i),
                        Colname.balance_responsible_id: str(i),
                        Colname.energy_supplier_id: str(i),
                        Colname.time_window: {
                            Colname.start: default_obs_time + timedelta(hours=i),
                            Colname.end: default_obs_time + timedelta(hours=i + 1),
                        },
                        Colname.sum_quantity: Decimal(13 + i),
                        Colname.quality: TimeSeriesQuality.ESTIMATED.value,
                        Colname.resolution: MeteringPointResolution.HOUR.value,
                        Colname.metering_point_type: MeteringPointType.CONSUMPTION.value,
                    },
                    ignore_index=True,
                )
            return spark.createDataFrame(
                pandas_df, schema=agg_consumption_and_production_schema
            )
        elif agg_method == AggregationMethod.FLEX_CONSUMPTION:
            pandas_df = pd.DataFrame(
                {
                    Colname.grid_area: [],
                    Colname.balance_responsible_id: [],
                    Colname.energy_supplier_id: [],
                    Colname.time_window: [],
                    Colname.sum_quantity: [],
                    Colname.quality: [],
                    Colname.resolution: [],
                    Colname.metering_point_type: [],
                }
            )
            for i in range(10):
                pandas_df = pandas_df.append(
                    {
                        Colname.grid_area: str(i),
                        Colname.balance_responsible_id: str(i),
                        Colname.energy_supplier_id: str(i),
                        Colname.time_window: {
                            Colname.start: default_obs_time + timedelta(hours=i),
                            Colname.end: default_obs_time + timedelta(hours=i + 1),
                        },
                        Colname.sum_quantity: Decimal(14 + i),
                        Colname.quality: TimeSeriesQuality.ESTIMATED.value,
                        Colname.resolution: MeteringPointResolution.HOUR.value,
                        Colname.metering_point_type: MeteringPointType.CONSUMPTION.value,
                    },
                    ignore_index=True,
                )
            return spark.createDataFrame(
                pandas_df, schema=agg_consumption_and_production_schema
            )
        elif agg_method == AggregationMethod.PRODUCTION:
            pandas_df = pd.DataFrame(
                {
                    Colname.grid_area: [],
                    Colname.balance_responsible_id: [],
                    Colname.energy_supplier_id: [],
                    Colname.time_window: [],
                    Colname.sum_quantity: [],
                    Colname.quality: [],
                    Colname.resolution: [],
                    Colname.metering_point_type: [],
                }
            )
            for i in range(10):
                pandas_df = pandas_df.append(
                    {
                        Colname.grid_area: str(i),
                        Colname.balance_responsible_id: str(i),
                        Colname.energy_supplier_id: str(i),
                        Colname.time_window: {
                            Colname.start: default_obs_time + timedelta(hours=i),
                            Colname.end: default_obs_time + timedelta(hours=i + 1),
                        },
                        Colname.sum_quantity: Decimal(50 + i),
                        Colname.quality: TimeSeriesQuality.ESTIMATED.value,
                        Colname.resolution: MeteringPointResolution.HOUR.value,
                        Colname.metering_point_type: MeteringPointType.PRODUCTION.value,
                    },
                    ignore_index=True,
                )
            return spark.createDataFrame(
                pandas_df, schema=agg_consumption_and_production_schema
            )

    return factory


@pytest.fixture(scope="module")
def agg_net_exchange_factory(
    spark: SparkSession, agg_net_exchange_schema: StructType
) -> Callable[[], DataFrame]:
    def factory() -> DataFrame:
        pandas_df = pd.DataFrame(
            {
                Colname.grid_area: ["1", "1", "1", "2", "2", "3"],
                Colname.time_window: [
                    {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 1, 0),
                        Colname.end: datetime(2020, 1, 1, 2, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 2, 0),
                        Colname.end: datetime(2020, 1, 1, 3, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 1, 0),
                        Colname.end: datetime(2020, 1, 1, 2, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                ],
                Colname.sum_quantity: [
                    Decimal(1.0),
                    Decimal(1.0),
                    Decimal(1.0),
                    Decimal(1.0),
                    Decimal(1.0),
                    Decimal(1.0),
                ],
                Colname.quality: ["56", "56", "56", "56", "56", "56"],
                Colname.resolution: [
                    MeteringPointResolution.HOUR.value,
                    MeteringPointResolution.HOUR.value,
                    MeteringPointResolution.HOUR.value,
                    MeteringPointResolution.HOUR.value,
                    MeteringPointResolution.HOUR.value,
                    MeteringPointResolution.HOUR.value,
                ],
                Colname.metering_point_type: [
                    MeteringPointType.EXCHANGE.value,
                    MeteringPointType.EXCHANGE.value,
                    MeteringPointType.EXCHANGE.value,
                    MeteringPointType.EXCHANGE.value,
                    MeteringPointType.EXCHANGE.value,
                    MeteringPointType.EXCHANGE.value,
                ],
            }
        )

        return spark.createDataFrame(pandas_df, schema=agg_net_exchange_schema)

    return factory


@pytest.fixture(scope="module")
def agg_flex_consumption_factory(
    spark: SparkSession, agg_consumption_and_production_schema: StructType
) -> Callable[[], DataFrame]:
    def factory() -> DataFrame:
        pandas_df = pd.DataFrame(
            {
                Colname.grid_area: ["1", "1", "1", "2", "2", "3"],
                Colname.balance_responsible_id: ["1", "2", "2", "1", "2", "1"],
                Colname.energy_supplier_id: ["1", "1", "2", "1", "1", "1"],
                Colname.time_window: [
                    {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 1, 0),
                        Colname.end: datetime(2020, 1, 1, 2, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 2, 0),
                        Colname.end: datetime(2020, 1, 1, 3, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 1, 0),
                        Colname.end: datetime(2020, 1, 1, 2, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                ],
                Colname.sum_quantity: [
                    Decimal(2.0),
                    Decimal(6.0),
                    Decimal(4.0),
                    Decimal(8.0),
                    Decimal(1.0),
                    Decimal(2.0),
                ],
                Colname.quality: ["56", "56", "56", "56", "56", "56"],
                Colname.resolution: [
                    MeteringPointResolution.HOUR.value,
                    MeteringPointResolution.HOUR.value,
                    MeteringPointResolution.HOUR.value,
                    MeteringPointResolution.HOUR.value,
                    MeteringPointResolution.HOUR.value,
                    MeteringPointResolution.HOUR.value,
                ],
                Colname.metering_point_type: [
                    MeteringPointType.CONSUMPTION.value,
                    MeteringPointType.CONSUMPTION.value,
                    MeteringPointType.CONSUMPTION.value,
                    MeteringPointType.CONSUMPTION.value,
                    MeteringPointType.CONSUMPTION.value,
                    MeteringPointType.CONSUMPTION.value,
                ],
            }
        )

        return spark.createDataFrame(
            pandas_df, schema=agg_consumption_and_production_schema
        )

    return factory


@pytest.fixture(scope="module")
def agg_hourly_consumption_factory(
    spark: SparkSession, agg_consumption_and_production_schema: StructType
) -> Callable[[], DataFrame]:
    def factory() -> DataFrame:
        pandas_df = pd.DataFrame(
            {
                Colname.grid_area: ["1", "1", "1", "2", "2", "3"],
                Colname.balance_responsible_id: ["1", "2", "2", "1", "2", "1"],
                Colname.energy_supplier_id: ["1", "1", "2", "1", "1", "1"],
                Colname.time_window: [
                    {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 1, 0),
                        Colname.end: datetime(2020, 1, 1, 2, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                ],
                Colname.sum_quantity: [
                    Decimal(6.0),
                    Decimal(1.0),
                    Decimal(4.0),
                    Decimal(2.0),
                    Decimal(3.0),
                    Decimal(1.0),
                ],
                Colname.quality: ["56", "56", "56", "56", "56", "56"],
                Colname.resolution: [
                    MeteringPointResolution.HOUR.value,
                    MeteringPointResolution.HOUR.value,
                    MeteringPointResolution.HOUR.value,
                    MeteringPointResolution.HOUR.value,
                    MeteringPointResolution.HOUR.value,
                    MeteringPointResolution.HOUR.value,
                ],
                Colname.metering_point_type: [
                    MeteringPointType.CONSUMPTION.value,
                    MeteringPointType.CONSUMPTION.value,
                    MeteringPointType.CONSUMPTION.value,
                    MeteringPointType.CONSUMPTION.value,
                    MeteringPointType.CONSUMPTION.value,
                    MeteringPointType.CONSUMPTION.value,
                ],
            }
        )

        return spark.createDataFrame(
            pandas_df, schema=agg_consumption_and_production_schema
        )

    return factory


@pytest.fixture(scope="module")
def agg_hourly_production_factory(
    spark: SparkSession, agg_consumption_and_production_schema: StructType
) -> Callable[[], DataFrame]:
    def factory() -> DataFrame:
        pandas_df = pd.DataFrame(
            {
                Colname.grid_area: ["1", "1", "1", "2", "2", "3"],
                Colname.balance_responsible_id: ["1", "2", "2", "1", "2", "1"],
                Colname.energy_supplier_id: ["1", "1", "2", "1", "1", "1"],
                Colname.time_window: [
                    {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 1, 0),
                        Colname.end: datetime(2020, 1, 1, 2, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                ],
                Colname.sum_quantity: [
                    Decimal(9.0),
                    Decimal(3.0),
                    Decimal(6.0),
                    Decimal(3.0),
                    Decimal(1.0),
                    Decimal(2.0),
                ],
                Colname.quality: ["56", "56", "56", "56", "56", "56"],
                Colname.resolution: [
                    MeteringPointResolution.HOUR.value,
                    MeteringPointResolution.HOUR.value,
                    MeteringPointResolution.HOUR.value,
                    MeteringPointResolution.HOUR.value,
                    MeteringPointResolution.HOUR.value,
                    MeteringPointResolution.HOUR.value,
                ],
                Colname.metering_point_type: [
                    MeteringPointType.PRODUCTION.value,
                    MeteringPointType.PRODUCTION.value,
                    MeteringPointType.PRODUCTION.value,
                    MeteringPointType.PRODUCTION.value,
                    MeteringPointType.PRODUCTION.value,
                    MeteringPointType.PRODUCTION.value,
                ],
            }
        )

        return spark.createDataFrame(
            pandas_df, schema=agg_consumption_and_production_schema
        )

    return factory


def test_grid_loss_calculation(
    agg_result_factory: Callable[[AggregationMethod], DataFrame]
) -> None:
    net_exchange_per_ga = create_dataframe_from_aggregation_result_schema(
        agg_result_factory(AggregationMethod.NET_EXCHANGE)
    )
    non_profiled_consumption = create_dataframe_from_aggregation_result_schema(
        agg_result_factory(AggregationMethod.HOURLY_CONSUMPTION)
    )
    flex_consumption = create_dataframe_from_aggregation_result_schema(
        agg_result_factory(AggregationMethod.FLEX_CONSUMPTION)
    )
    production = create_dataframe_from_aggregation_result_schema(
        agg_result_factory(AggregationMethod.PRODUCTION)
    )

    result = calculate_grid_loss(
        net_exchange_per_ga, non_profiled_consumption, flex_consumption, production
    )

    # Verify the calculation result is correct by checking 50+i + 20+i - (13+i + 14+i) equals 43 for all i in range 0 to 9
    assert result.filter(col(Colname.sum_quantity) != 43).count() == 0

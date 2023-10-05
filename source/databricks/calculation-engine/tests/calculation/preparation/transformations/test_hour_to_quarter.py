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
from package.constants import Colname
from pyspark.sql import SparkSession, Row

from package.codelists import (
    MeteringPointResolution,
    TimeSeriesQuality,
)
from pyspark.sql.types import (
    StructType,
    StringType,
    DecimalType,
    TimestampType,
    StructField,
)
from package.calculation.preparation.transformations.hour_to_quarter import (
    transform_hour_to_quarter,
)


basis_data_time_series_points_schema = StructType(
    [
        StructField(Colname.grid_area, StringType(), True),
        StructField(Colname.to_grid_area, StringType(), True),
        StructField(Colname.from_grid_area, StringType(), True),
        StructField(Colname.metering_point_id, StringType(), True),
        StructField(Colname.metering_point_type, StringType(), True),
        StructField(Colname.resolution, StringType(), True),
        StructField(Colname.observation_time, TimestampType(), True),
        StructField(Colname.quantity, StringType(), True),
        StructField(Colname.quality, StringType(), True),
        StructField(Colname.energy_supplier_id, StringType(), True),
        StructField(Colname.balance_responsible_id, StringType(), True),
    ]
)

time_series_quarter_points_schema = StructType(
    [
        StructField(Colname.grid_area, StringType(), True),
        StructField(Colname.to_grid_area, StringType(), True),
        StructField(Colname.from_grid_area, StringType(), True),
        StructField(Colname.metering_point_id, StringType(), True),
        StructField(Colname.metering_point_type, StringType(), True),
        StructField(Colname.resolution, StringType(), True),
        StructField(Colname.observation_time, TimestampType(), True),
        StructField(Colname.quantity, DecimalType(18, 6), True),
        StructField(Colname.quality, StringType(), True),
        StructField(Colname.energy_supplier_id, StringType(), True),
        StructField(Colname.balance_responsible_id, StringType(), True),
        StructField("quarter_time", TimestampType(), True),
        StructField(
            Colname.time_window,
            StructType(
                [
                    StructField(Colname.start, TimestampType()),
                    StructField(Colname.end, TimestampType()),
                ]
            ),
            False,
        ),
        StructField("quarter_quantity", DecimalType(18, 6), True),
    ]
)


def _create_enriched_time_series_row(
    grid_area: int = "805",
    to_grid_area: str = "805",
    from_grid_area: str = "806",
    metering_point_id: str = "the_metering_point_id",
    metering_point_type: str = "the_metering_point_type",
    resolution: str = MeteringPointResolution.HOUR.value,
    observation_time: TimestampType() = datetime(2020, 1, 1, 0, 0),
    quantity: str = Decimal("4.444444"),
    quality: str = TimeSeriesQuality.ESTIMATED.value,
    energy_supplier_id: str = "the_energy_supplier_id",
    balance_responsible_id: str = "the_balance_responsible_id",
) -> Row:
    row = {
        Colname.grid_area: grid_area,
        Colname.to_grid_area: to_grid_area,
        Colname.from_grid_area: from_grid_area,
        Colname.metering_point_id: metering_point_id,
        Colname.metering_point_type: metering_point_type,
        Colname.resolution: resolution,
        Colname.observation_time: observation_time,
        Colname.quantity: quantity,
        Colname.quality: quality,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.balance_responsible_id: balance_responsible_id,
    }

    return Row(**row)


def test__transform_hour_to_quarter__split_hourly_enriched_time_series(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [_create_enriched_time_series_row()]
    enriched_time_series = spark.createDataFrame(
        rows, basis_data_time_series_points_schema
    )

    # Act
    enriched_time_series_all_quarter = transform_hour_to_quarter(enriched_time_series)

    # Assert
    assert enriched_time_series_all_quarter.count() == 4
    assert enriched_time_series_all_quarter.schema == time_series_quarter_points_schema
    assert enriched_time_series_all_quarter.collect()[0]["quarter_quantity"] == Decimal(
        "1.111111"
    )

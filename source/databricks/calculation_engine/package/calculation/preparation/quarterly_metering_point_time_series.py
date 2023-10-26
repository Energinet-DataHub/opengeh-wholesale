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

from pyspark.sql import DataFrame
import pyspark.sql.functions as f
import pyspark.sql.types as t

from package.calculation.energy.schemas import (
    time_series_quarter_points_schema,
)
from package.common import DataFrameWrapper, assert_schema
from package.constants import Colname


class QuarterlyMeteringPointTimeSeries(DataFrameWrapper):
    """
    Time series points of metering points with resolution quarterly.

    The points are enriched with metering point data required by calculations.

    When points are missing the time series are padded with
    points where quantity=0 and quality=missing.

    Can be either hourly or quarterly.
    """

    def __init__(self, df: DataFrame):
        # TODO BJM: Remove this temp workaround?
        df = self._add_missing_nullable_columns(df)

        df = df.select(
            Colname.grid_area,
            Colname.to_grid_area,
            Colname.from_grid_area,
            Colname.metering_point_id,
            Colname.metering_point_type,
            # TODO BJM: Does it make sense to require a resolution col in a "quarterly type"
            Colname.resolution,
            # TODO BJM: Does it make sense to have both observation_time, quarter_time, time_window and resolution?
            Colname.observation_time,
            Colname.quantity,
            Colname.quality,
            Colname.energy_supplier_id,
            Colname.balance_responsible_id,
            Colname.quarter_time,
            Colname.time_window,
            Colname.quarter_quantity,
        )

        # Workaround to enforce quantity nullable=False. This should be safe as quantity in input is nullable=False
        df.schema[Colname.quantity].nullable = False

        assert_schema(
            df.schema,
            time_series_quarter_points_schema,
            ignore_column_order=True,
            ignore_nullability=True,
            ignore_decimal_scale=True,
            ignore_decimal_precision=True,
        )

        super().__init__(df)

    # TODO BJM: Make generic and reuse
    @staticmethod
    def _add_missing_nullable_columns(result: DataFrame) -> DataFrame:
        if Colname.to_grid_area not in result.columns:
            result = result.withColumn(
                Colname.to_grid_area, f.lit(None).cast(t.StringType())
            )
        if Colname.from_grid_area not in result.columns:
            result = result.withColumn(
                Colname.from_grid_area, f.lit(None).cast(t.StringType())
            )
        if Colname.energy_supplier_id not in result.columns:
            result = result.withColumn(
                Colname.energy_supplier_id, f.lit(None).cast(t.StringType())
            )
        if Colname.balance_responsible_id not in result.columns:
            result = result.withColumn(
                Colname.balance_responsible_id, f.lit(None).cast(t.StringType())
            )
        return result

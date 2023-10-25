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
    aggregation_result_schema,
)
from package.common import TimeSeries, assert_schema
from package.constants import Colname


class EnergyResults(TimeSeries):
    """
    Time series of energy results.
    """

    def __init__(self, df: DataFrame):
        """Fit data frame in a general DataFrame. This is used for all results and missing columns will be null."""

        df = self._add_missing_nullable_columns(df)
        df = df.na.fill(value=0, subset=[Colname.sum_quantity])

        df = df.select(
            Colname.grid_area,
            Colname.to_grid_area,
            Colname.from_grid_area,
            Colname.balance_responsible_id,
            Colname.energy_supplier_id,
            Colname.time_window,
            Colname.sum_quantity,
            Colname.qualities,
            Colname.metering_point_type,
            Colname.settlement_method,
        )

        assert_schema(
            df.schema,
            aggregation_result_schema,
            ignore_nullability=True,
            ignore_column_order=True,
            ignore_decimal_scale=True,
            ignore_decimal_precision=True,
        )

        super().__init__(df, aggregation_result_schema)

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
        if Colname.balance_responsible_id not in result.columns:
            result = result.withColumn(
                Colname.balance_responsible_id, f.lit(None).cast(t.StringType())
            )
        if Colname.energy_supplier_id not in result.columns:
            result = result.withColumn(
                Colname.energy_supplier_id, f.lit(None).cast(t.StringType())
            )
        if Colname.settlement_method not in result.columns:
            result = result.withColumn(
                Colname.settlement_method, f.lit(None).cast(t.StringType())
            )
        return result

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

import pyspark.sql.types as t
from pyspark.sql import DataFrame

from package.common import DataFrameWrapper, assert_schema
from package.constants import Colname


class EnergyResults(DataFrameWrapper):
    """
    Time series of energy results.
    """

    def __init__(self, df: DataFrame):
        """Fit data frame in a general DataFrame. This is used for all results and missing columns will be null."""

        df = DataFrameWrapper._add_missing_nullable_columns(df, energy_results_schema)

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
            energy_results_schema,
            ignore_nullability=True,
            ignore_column_order=True,
            ignore_decimal_scale=True,
            ignore_decimal_precision=True,
        )

        super().__init__(df)


energy_results_schema = t.StructType(
    [
        t.StructField(Colname.grid_area, t.StringType(), False),
        t.StructField(Colname.to_grid_area, t.StringType(), True),
        t.StructField(Colname.from_grid_area, t.StringType(), True),
        t.StructField(Colname.balance_responsible_id, t.StringType(), True),
        t.StructField(Colname.energy_supplier_id, t.StringType(), True),
        t.StructField(
            Colname.time_window,
            t.StructType(
                [
                    t.StructField(Colname.start, t.TimestampType()),
                    t.StructField(Colname.end, t.TimestampType()),
                ]
            ),
            False,
        ),
        t.StructField(Colname.sum_quantity, t.DecimalType(18, 3), False),
        t.StructField(Colname.qualities, t.ArrayType(t.StringType(), False), False),
        t.StructField(Colname.metering_point_type, t.StringType(), False),
        t.StructField(Colname.settlement_method, t.StringType(), True),
    ]
)

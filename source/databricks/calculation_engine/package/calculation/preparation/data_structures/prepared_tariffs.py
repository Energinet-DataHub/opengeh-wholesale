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

from package.common import DataFrameWrapper
from package.constants import Colname


class PreparedTariffs(DataFrameWrapper):
    """
    Represents tariffs that are prepared for calculation.
    The 'sum_quantity' column is the sum energy quantities within the time window (represented by charge_time and resolution).
    Missing prices are represented by a None value in the 'charge_price' column.
    """

    def __init__(self, df: DataFrame):
        super().__init__(
            df,
            prepared_tariffs_schema,
            # We ignore_nullability because it has turned out to be too hard and even possibly
            # introducing more errors than solving in order to stay in exact sync with the
            # logically correct schema.
            ignore_nullability=True,
            ignore_decimal_scale=True,
            ignore_decimal_precision=True,
        )


# The nullability and decimal types are not precisely representative of the actual data frame schema at runtime,
# See comments to the `assert_schema()` invocation.
prepared_tariffs_schema = t.StructType(
    [
        t.StructField(Colname.charge_key, t.StringType(), False),
        t.StructField(Colname.charge_code, t.StringType(), False),
        t.StructField(Colname.charge_type, t.StringType(), False),
        t.StructField(Colname.charge_owner, t.StringType(), False),
        t.StructField(Colname.charge_tax, t.BooleanType(), False),
        t.StructField(Colname.resolution, t.StringType(), False),
        t.StructField(Colname.charge_time, t.TimestampType(), False),
        t.StructField(Colname.charge_price, t.DecimalType(18, 6), True),
        t.StructField(Colname.metering_point_id, t.StringType(), False),
        t.StructField(Colname.energy_supplier_id, t.StringType(), False),
        t.StructField(Colname.metering_point_type, t.StringType(), False),
        t.StructField(Colname.settlement_method, t.StringType(), True),
        t.StructField(Colname.grid_area, t.StringType(), False),
        # quantity is a sum of all quantities within the time window (represented by charge_time and resolution)
        t.StructField(Colname.quantity, t.DecimalType(18, 3), True),
        t.StructField(Colname.qualities, t.ArrayType(t.StringType()), False),
    ]
)

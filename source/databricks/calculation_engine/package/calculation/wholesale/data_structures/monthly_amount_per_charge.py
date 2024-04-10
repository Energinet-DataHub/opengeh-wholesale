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


class MonthlyAmountPerCharge(DataFrameWrapper):
    """
    This is the result of the sum of amounts per charge, which is the sum of amounts within a month and across metering points.
    """

    def __init__(self, df: DataFrame):
        """
        Fit data frame in a general DataFrame. This is used for all results and missing columns will be null.
        """

        super().__init__(
            df,
            monthly_amount_per_charge_schema,
            # We ignore_nullability because it has turned out to be too hard and even possibly
            # introducing more errors than solving in order to stay in exact sync with the
            # logically correct schema.
            ignore_nullability=True,
            ignore_decimal_scale=True,
            ignore_decimal_precision=True,
        )


# The nullability and decimal types are not precisely representative of the actual data frame schema at runtime,
# See comments to the `assert_schema()` invocation.
monthly_amount_per_charge_schema = t.StructType(
    [
        t.StructField(Colname.grid_area, t.StringType(), False),
        t.StructField(Colname.energy_supplier_id, t.StringType(), False),
        t.StructField(Colname.unit, t.StringType(), False),
        t.StructField(Colname.charge_time, t.TimestampType(), False),
        t.StructField(Colname.total_amount, t.DecimalType(18, 6), True),
        t.StructField(Colname.charge_tax, t.BooleanType(), False),
        t.StructField(Colname.charge_code, t.StringType(), False),
        t.StructField(Colname.charge_type, t.StringType(), False),
        t.StructField(Colname.charge_owner, t.StringType(), False),
    ]
)

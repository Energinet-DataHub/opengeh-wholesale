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

from package.codelists import ChargeType
from package.common import DataFrameWrapper
from package.constants import Colname


class ChargeMasterData(DataFrameWrapper):
    """
    Represents the charge master data.
    All periods are clamped to least common period of the metering point and the charge master data period.
    """

    def __init__(self, df: DataFrame):
        super().__init__(
            df,
            charge_master_data_schema,
            # We ignore_nullability because it has turned out to be too hard and even possibly
            # introducing more errors than solving in order to stay in exact sync with the
            # logically correct schema.
            ignore_nullability=True,
            ignore_decimal_scale=True,
            ignore_decimal_precision=True,
        )

    def filter_by_charge_type(self, charge_type: ChargeType) -> "ChargeMasterData":
        df = self._df.filter(self._df[Colname.charge_type] == charge_type.value)
        return ChargeMasterData(df)


# The nullability and decimal types are not precisely representative of the actual data frame schema at runtime,
# See comments to the `assert_schema()` invocation.
charge_master_data_schema = t.StructType(
    [
        t.StructField(Colname.charge_key, t.StringType(), False),
        t.StructField(Colname.charge_code, t.StringType(), False),
        t.StructField(Colname.charge_type, t.StringType(), False),
        t.StructField(Colname.charge_owner, t.StringType(), False),
        t.StructField(Colname.charge_tax, t.BooleanType(), False),
        t.StructField(Colname.resolution, t.StringType(), False),
        t.StructField(Colname.from_date, t.TimestampType(), True),
        t.StructField(Colname.to_date, t.TimestampType(), True),
    ]
)

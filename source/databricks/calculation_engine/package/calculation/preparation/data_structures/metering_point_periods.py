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


class MeteringPointPeriods(DataFrameWrapper):
    """
    Represents metering point periods.
    """

    def __init__(self, df: DataFrame):
        super().__init__(
            df,
            metering_point_periods_schema,
        )


metering_point_periods_schema = t.StructType(
    [
        t.StructField(Colname.calculation_id, t.StringType(), False),
        t.StructField(Colname.metering_point_id, t.StringType(), False),
        t.StructField(Colname.metering_point_type, t.StringType(), False),
        t.StructField(Colname.settlement_method, t.StringType(), True),
        t.StructField(Colname.grid_area_code, t.StringType(), False),
        t.StructField(Colname.resolution, t.StringType(), False),
        t.StructField(Colname.from_grid_area_code, t.StringType(), True),
        t.StructField(Colname.to_grid_area_code, t.StringType(), True),
        t.StructField(Colname.parent_metering_point_id, t.StringType(), True),
        t.StructField(Colname.energy_supplier_id, t.StringType(), True),
        t.StructField(Colname.balance_responsible_party_id, t.StringType(), True),
        t.StructField(Colname.from_date, t.TimestampType(), False),
        t.StructField(Colname.to_date, t.TimestampType(), False),
    ]
)

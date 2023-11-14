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


class GridLossResponsible(DataFrameWrapper):
    """
    Grid loss responsible.
    """

    def __init__(self, df: DataFrame):
        super().__init__(
            df,
            grid_loss_responsible_schema,
            ignore_nullability=True,
        )


grid_loss_responsible_schema = t.StructType(
    [
        t.StructField(Colname.metering_point_id, t.StringType(), False),
        t.StructField(Colname.grid_area, t.StringType(), False),
        t.StructField(Colname.from_date, t.TimestampType(), False),
        t.StructField(Colname.to_date, t.TimestampType(), True),
        t.StructField(Colname.metering_point_type, t.StringType(), False),
        t.StructField(Colname.energy_supplier_id, t.StringType(), False),
        t.StructField(Colname.is_negative_grid_loss_responsible, t.BooleanType(), True),
        t.StructField(Colname.is_positive_grid_loss_responsible, t.BooleanType(), True),
    ]
)

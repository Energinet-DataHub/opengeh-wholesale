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


class GridLossMeteringPointIds(DataFrameWrapper):
    """
    Represents grid loss metering points.
    """

    def __init__(self, df: DataFrame):
        super().__init__(
            df,
            grid_loss_metering_point_ids_schema,
        )


# The nullability and decimal types are not precisely representative of the actual data frame schema at runtime,
# See comments to the `assert_schema()` invocation.
grid_loss_metering_point_ids_schema = t.StructType(
    [
        t.StructField(Colname.metering_point_id, t.StringType(), False),
    ]
)

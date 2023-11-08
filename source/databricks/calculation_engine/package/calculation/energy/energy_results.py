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


class EnergyResults(DataFrameWrapper):
    """
    Time series of energy results.
    """

    def __init__(self, df: DataFrame):
        """
        Fit data frame in a general DataFrame. This is used for all results and missing columns will be null.
        """

        super().__init__(
            df,
            energy_results_schema,
            # TODO BJM: These should eventually all be set to False
            ignore_nullability=True,
            ignore_decimal_scale=True,
            ignore_decimal_precision=True,
        )


energy_results_schema = t.StructType(
    [
        t.StructField(Colname.grid_area, t.StringType(), False),
        t.StructField(Colname.to_grid_area, t.StringType(), True),
        t.StructField(Colname.from_grid_area, t.StringType(), True),
        t.StructField(Colname.balance_responsible_id, t.StringType(), True),
        t.StructField(Colname.energy_supplier_id, t.StringType(), True),
        # Suggestion: Why not just a single time stamp?
        #           That is much simpler to manage throughout the code and especially in all the tests
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

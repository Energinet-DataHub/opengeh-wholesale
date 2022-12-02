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

from pyspark.sql.types import (
    StructField,
    StringType,
    TimestampType,
    StructType,
)

"""
**DEPRECATED** - see the [README](../README.md).

Schema for grid area periods

Data must be stored in a Delta table.
The table need not be partitioned.
The table data must always contain updated periods.

Grid access provider is not needed as it is related to B2B messaging.
"""
grid_area_period_schema = StructType(
    [
        # 3 character grid area code uniquely identifying the grid area. All characters must be digits (0-9).
        StructField("GridAreaCode", StringType(), True),

        # The start date of the period. The start date must be the UTC time of the begining of a date in the given timezone/DST.
        # The date is inclusive.
        StructField("FromDate", TimestampType(), True),
        
        # The to-date of the period. The to-date must be the UTC time of the begining of a date in the given timezone/DST.
        # The moment is exclusive.
        # The date of the last period is null.
        StructField("ToDate", TimestampType(), True),
    ]
)

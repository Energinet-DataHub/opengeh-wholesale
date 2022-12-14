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

Schema for system correction and grid loss metering point periods

NOTE: This separate table will be skipped if the data can fit in
      the metering point period schema (and by adding a field to identify normal|GLMP|SCMP type).

Metering point periods are used in balance fixing and settlement.
Periods (given by `FromDate` and `ToDate`) must not overlap and must not have gaps in between.
All but the `ToDate` of the last period must have value. The `ToDate` of the last period is null.

Data must be stored in a Delta table.
The table need not be partitioned.
The table data must always contain updated periods.
"""
system_correction_and_grid_loss_metering_point_periods_schema = StructType(
    [
        # The metering point GSRN number (18 characters) that uniquely identifies the metering point.
        # Example: 578710000000000103
        StructField("MeteringPointId", StringType(), False),
        
        # "system_correction" | "grid_loss"
        # Example: system_correction
        StructField("Type", StringType(), False),
        
        # 3 character grid area code uniquely identifying the grid area. All characters must be digits (0-9).
        # Used in balance fixing and settlement.
        # Example: 805
        StructField("GridAreaCode", StringType(), False),
        
        # The start date of the period. The start date must be the UTC time of the beginning of a date in the given timezone/DST.
        # The date is inclusive.
        StructField("FromDate", TimestampType(), False),
        
        # The to-date of the period. The to-date must be the UTC time of the beginning of a date in the given timezone/DST.
        # The moment is exclusive.
        # The date of the last period is null.
        StructField("ToDate", TimestampType(), True),
    ]
)

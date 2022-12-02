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
Schema for metering point periods

Metering point periods are used in balance fixing and settlement. Some fields are only used in settlement. See each field for details.
Periods (given by `FromDate` and `ToDate`) must not overlap and must not have gaps in between.
All but the `ToDate` of the last period must have value. The `ToDate` of the last period is null.

Only periods where metering points are connected (E22) or disconnected (E23) are included.

Data must be stored in a Delta table.
The table holds all consumption, production, exchange, and child metering points.

The table must be partitioned by `ToDate`: ToDate_Year/ToDate_Month/ToDate_Date.
It is important to partition by to-date instead of from-date as it will ensure efficient data filtering.
This is because most periods will have a to-date prior to the calculation period start date.

The table data must always contain updated periods.
"""
metering_point_period_schema = StructType(
    [
        # GSRN (18 characters) that uniquely identifies the metering point
        # Used in balance fixing and settlement.
        # Example: 578710000000000103
        StructField("MeteringPointId", StringType(), False),
        
        # "E17" (consumption) | "E18" (production) | "E20" (exchange) | "D01"-"D99" (child)
        # Used in balance fixing and settlement. However, child metering points are only used in settlement.
        # Example: E20
        StructField("Type", StringType(), False),
        
        # "system-correction" | "grid-loss"
        # For non-calculated metering points the calculation type is null.
        # Example: system-correction
        StructField("CalculationType", StringType(), True),

        # "E02" (non-profiled)| "D01" (flex)
        # When metering point is not a consumption (E17) metering point the value is null. Otherwise it must have a value.
        # Used in balance fixing and settlement.
        # 
        # Example: D01
        StructField("SettlementMethod", StringType(), True),
        
        # 3 character grid area code uniquely identifying the grid area. All characters must be digits (0-9).
        # Used in balance fixing and settlement.
        # Example: 805
        StructField("GridAreaCode", StringType(), False),
        
        # "PT1H" (hourly) | "PT15M" (quarterly)
        # Used in balance fixing and settlement.
        # Example: PT1H
        StructField("Resolution", StringType(), False),
        
        # 3 character grid area code uniquely identifying the from-grid area. All characters must be digits (0-9).
        # The code has a value for E20 (exchange) metering points. For all other types it's null.
        # Used in balance fixing and settlement.
        # Example: 122
        StructField("FromGridAreaCode", StringType(), True),
        
        # 3 character grid area code uniquely identifying the to-grid area. All characters must be digits (0-9).
        # The code has a value for E20 (exchange) metering points. For all other types it's null.
        # Used in balance fixing and settlement.
        # Example: 134
        StructField("ToGridAreaCode", StringType(), True),
        
        # The id of the parent metering point or null.
        # Used only in settlement.
        # Example: 578710000000000112
        StructField("ParentMeteringPointId", StringType(), True),
        
        # The unique GLN/EIC number of the energy supplier
        # Not all metering points have an energy supplier registered, e.g. exchange (E20)
        # Used in balance fixing and settlement.
        # Example: 8100000000108
        StructField("EnergySupplierId", StringType(), True),

        # The unique GLN/EIC number of the balance responsible
        # Not all metering points have a balance responsible registered, e.g. exchange (E20)
        # Used in balance fixing and settlement.
        # Example: 8100000000109
        StructField("BalanceResponsibleId", StringType(), True),

        # The start date of the period. The start date must be the UTC time of the begining of a date in the given timezone/DST.
        # The date is inclusive.
        # Used in balance fixing and settlement.
        StructField("FromDate", TimestampType(), False),
        
        # The to-date of the period. The to-date must be the UTC time of the begining of a date in the given timezone/DST.
        # The moment is exclusive.
        # The date of the last period is null.
        # Used in balance fixing and settlement.
        StructField("ToDate", TimestampType(), True),
       
        # The year part of the `ToDate`. Used for partitioning.
        StructField("ToDate_Year", IntegerType(), True),
        
        # The month part of the `ToDate`. Used for partitioning.
        StructField("ToDate_Month", IntegerType(), True),
        
        # The date part of the `ToDate`. Used for partitioning.
        StructField("ToDate_Date", IntegerType(), True),
    ]
)

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

Data must be stored in a Delta table.
The table holds all consumption, production, exchange, and child metering points.
The table must be partitioned by `ToDate`: ToDate_Year/ToDate_Month/ToDate_Date.
TODO explain why by to-date
The table data must always contain updated periods.
"""
metering_point_period_schema = StructType(
    [
        # The metering point GSRN number (18 characters) that uniquely identifies the metering point
        StructField("MeteringPointId", StringType(), True),
        
        # "E17" (consumption) | "E18" (production) | "E20" (exchange) | "D01"-"D99" (child)
        StructField("Type", StringType(), True),
        
        # "E02" (quarterly)| "D01" (flex)
        StructField("SettlementMethod", StringType(), True),
        
        # 3 character grid area code uniquely identifying the grid area. All characters must be digits (0-9).
        StructField("GridAreaCode", StringType(), True),
        
        # "E22" (connected) | "E23" (disconnected) | "D03" (new) | "D02" (closed down)
        StructField("ConnectionState", StringType(), True),
        
        # "PT1H" (hourly) | "PT15M" (quarterly)
        StructField("Resolution", StringType(), True),
        
        # 3 character grid area code uniquely identifying the from-grid area. All characters must be digits (0-9).
        StructField("FromGridAreaCode", StringType(), True),
        
        # 3 character grid area code uniquely identifying the to-grid area. All characters must be digits (0-9).
        StructField("ToGridAreaCode", StringType(), True),
        
        # The id of the parent metering point
        StructField("ParentMeteringPointId", StringType(), True),
        
        # The unique actor id of the energy supplier (as provided by the market participant domain)
        StructField("EnergySupplierId", StringType(), True),

        # The start date of the period. The start date must be the UTC time of the begining of a date in the given timezone/DST.
        # The date is inclusive.
        StructField("FromDate", TimestampType(), True),
        
        # The to-date of the period. The to-date must be the UTC time of the begining of a date in the given timezone/DST.
        # The moment is exclusive.
        # The date of the last period is null.
        StructField("ToDate", TimestampType(), True),
       
        # The year part of the `ToDate`. Used in partition.
        StructField("ToDate_Year", IntegerType(), True),
        
        # The month part of the `ToDate`. Used in partition.
        StructField("ToDate_Month", IntegerType(), True),
        
        # The date part of the `ToDate`. Used in partition.
        StructField("ToDate_Date", IntegerType(), True),
    ]
)

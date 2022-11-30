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

Data must be stored in a Delta table.
The table holds all consumption, production, exchange, and child metering points.
The table must be partitioned by `ToDate`: ToDate_Year/ToDate_Month/ToDate_Date.
TODO explain why by to-date
The table data must always contain updated periods.
"""
metering_point_period_schema = StructType(
    [
        # The metering point GSRN number (18 characters) that uniquely identifies the metering point.
        # Used in balance fixing and settlement.
        # Example: 578710000000000103
        StructField("MeteringPointId", StringType(), False),
        
        # "E17" (consumption) | "E18" (production) | "E20" (exchange) | "D01"-"D99" (child)
        # Used in balance fixing and settlement. However, child metering points are only used in settlement.
        # Example: E20
        StructField("Type", StringType(), False),
        
        # "E02" (quarterly)| "D01" (flex)
        # Used in balance fixing and settlement.
        # Example: D01
        StructField("SettlementMethod", StringType(), False),
        
        # 3 character grid area code uniquely identifying the grid area. All characters must be digits (0-9).
        # Used in balance fixing and settlement.
        # Example: 805
        StructField("GridAreaCode", StringType(), False),
        
        # "E22" (connected) | "E23" (disconnected) | "D03" (new) | "D02" (closed down)
        # Used in balance fixing and settlement.
        # Example: E22
        StructField("ConnectionState", StringType(), False),
        
        # "PT1H" (hourly) | "PT15M" (quarterly)
        # Used in balance fixing and settlement.
        # Example: PT1H
        StructField("Resolution", StringType(), False),
        
        # 3 character grid area code uniquely identifying the from-grid area. All characters must be digits (0-9).
        # Used in balance fixing and settlement.
        # Example: 122
        StructField("FromGridAreaCode", StringType(), False),
        
        # 3 character grid area code uniquely identifying the to-grid area. All characters must be digits (0-9).
        # Used in balance fixing and settlement.
        # Example: 134
        StructField("ToGridAreaCode", StringType(), False),
        
        # The id of the parent metering point or null.
        # Used only in settlement.
        # Example: 578710000000000112
        StructField("ParentMeteringPointId", StringType(), True),
        
        # The unique GLN/EIC number of the energy supplier
        # Used in balance fixing and settlement.
        # Example: 8100000000108
        StructField("EnergySupplierId", StringType(), False),

        # The unique GLN/EIC number of the balance responsible
        # Used in balance fixing and settlement.
        # Example: 8100000000109
        StructField("BalanceResponsibleId", StringType(), False),

        # The start date of the period. The start date must be the UTC time of the begining of a date in the given timezone/DST.
        # The date is inclusive.
        # Used in balance fixing and settlement.
        StructField("FromDate", TimestampType(), False),
        
        # The to-date of the period. The to-date must be the UTC time of the begining of a date in the given timezone/DST.
        # The moment is exclusive.
        # The date of the last period is null.
        # Used in balance fixing and settlement.
        # TODO What happens with the partitioning if the date is null?
        StructField("ToDate", TimestampType(), False),
       
        # The year part of the `ToDate`. Used for partitioning.
        StructField("ToDate_Year", IntegerType(), False),
        
        # The month part of the `ToDate`. Used for partitioning.
        StructField("ToDate_Month", IntegerType(), False),
        
        # The date part of the `ToDate`. Used for partitioning.
        StructField("ToDate_Date", IntegerType(), False),
    ]
)

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
    BooleanType,
    DecimalType,
    StructField,
    StringType,
    TimestampType,
    StructType,
)

"""
Schema for charge periods

Charge periods are only used in settlement.
Periods (given by `FromDate` and `ToDate`) must not overlap and must not have gaps in between.
All but the `ToDate` of the last period must have value. The `ToDate` of the last period is null.

Data must be stored in a Delta table.

The table must be partitioned by `ToDate`: ToDate_Year/ToDate_Month/ToDate_Day.
It is important to partition by to-date instead of from-date as it will ensure efficient data filtering.
This is because most periods will have a to-date prior to the calculation period start date.

The table data must always contain updated periods.
"""
charge_period_schema = StructType(
    [
        # ID of the charge
        # The ID is only guaranteed to be unique for a specific actor and charge type.
        # The ID is provided by the charge owner (actor).
        # Example: 0010643756
        StructField("ChargeId", StringType(), False),

        # "D01" (subscription) | "D02 (fee) | "D03" (tariff)
        # Example: subscription
        StructField("ChargeType", StringType(), False),

        # The unique GLN/EIC number of the charge owner (actor)
        # Example: 8100000000030
        StructField("ChargeOwnerId", StringType(), False),

        # "PT1H" (hourly) | "PT1D" (daily)
        # Monthly values need to be converted to daily values before using in calculations.
        # Example: PT1H
        StructField("Resolution", StringType(), False),

        # Specifies whether the charge is tax. Applies only to tariffs.
        # For subscriptions and fees the value must be false.
        # Example: True
        StructField("IsTax", BooleanType(), False),

        # GSRN (18 characters) that uniquely identifies the metering point
        # The field is from the charge link.
        # Example: 578710000000000103
        StructField("MeteringPointId", StringType(), False),
        
        # Quantity (also known as factor)
        # Value is 1 or larger. For tariffs it's always 1.
        # The field is from the charge link.
        StructField("Quantity", IntegerType(), True),

        # The start date of the period. The start date must be the UTC time of the begining of a date in the given timezone/DST.
        # The date is inclusive.
        StructField("FromDate", TimestampType(), False),
        
        # The to-date of the period. The to-date must be the UTC time of the begining of a date in the given timezone/DST.
        # The moment is exclusive.
        # The date of the last period is null.
        StructField("ToDate", TimestampType(), True),
       
        # The year part of the `ToDate`. Used for partitioning.
        StructField("ToDate_Year", IntegerType(), True),
        
        # The month part of the `ToDate`. Used for partitioning.
        StructField("ToDate_Month", IntegerType(), True),
        
        # The day part (1-31) of the `ToDate`. Used for partitioning.
        StructField("ToDate_Day", IntegerType(), True),
    ]
)

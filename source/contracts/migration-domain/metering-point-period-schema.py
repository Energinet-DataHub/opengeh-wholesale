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

# Schema for metering point
# Data must be stored in a Delta table.
# The table must be partitioned by `FromDate`.
# The table data must always contain updated periods.
metering_point_period_schema = StructType(
    [
        # The metering point GSRN number that uniquely identifies the metering point
        # TODO Does DH2 have other ids than GSRN for metering points?
        StructField("MeteringPointId", StringType(), True),
        # "E17" | "E18" | "E20" | child?
        StructField("Type", StringType(), True),
        # TODO "" | "" | ""
        StructField("SettlementMethod", StringType(), True),
        # 3 character grid area code uniquely identifying the grid area. All characters must be digits (0-9).
        StructField("GridAreaCode", StringType(), True),
        # TODO "" | "" | ""
        StructField("ConnectionState", StringType(), True),
        # "PT1H" | "PT15M"
        StructField("Resolution", StringType(), True),
        # 3 character grid area code uniquely identifying the incoming grid area. All characters must be digits (0-9).
        StructField("InGridAreaCode", StringType(), True),
        # 3 character grid area code uniquely identifying the outgoing grid area. All characters must be digits (0-9).
        StructField("OutGridAreaCode", StringType(), True),
        # The metering point GSRN number that uniquely identifies the parent metering point
        StructField("ParentMeteringPointId", StringType(), True),
        # The start date of the period. The start date must be the UTC time of the begining of a date in the given timezone/DST.
        # The date is inclusive.
        # TODO Or effective date?
        StructField("FromDate", TimestampType(), True),
        # The to-date of the period. The to-date must be the UTC time of the begining of a date in the given timezone/DST.
        # The date is exclusive.
        StructField("ToDate", TimestampType(), True),
        # The unique actor id of the energy supplier.
        StructField("EnergySupplierId", StringType(), True),
        # The year part of the `ToDate`. Used in partition.
        StructField("ToDateYear", IntegerType(), True),
        # The month part of the `ToDate`. Used in partition.
        StructField("ToDateMonth", IntegerType(), True),
        # The date part of the `ToDate`. Used in partition.
        StructField("ToDateDate", IntegerType(), True),
    ]
)

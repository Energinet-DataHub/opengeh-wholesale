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
        StructField("SettlementMethod", StringType(), True),
        StructField("GridArea", StringType(), True),
        StructField("ConnectionState", StringType(), True),
        # "PT1H" | "PT15M"
        StructField("Resolution", StringType(), True),
        StructField("InGridArea", StringType(), True),
        StructField("OutGridArea", StringType(), True),
        StructField("ParentMeteringPointID", StringType(), True),
        # Or effective date?
        StructField("FromDate", StringType(), True),
        StructField("ToDate", StringType(), True),
        StructField("EnergySupplierId", StringType(), True),
        # The year part of the `ToDate`. Used in partition.
        StructField("ToDateYear", IntegerType(), True),
        # The month part of the `ToDate`. Used in partition.
        StructField("ToDateMonth", IntegerType(), True),
        # The date part of the `ToDate`. Used in partition.
        StructField("ToDateDate", IntegerType(), True),
    ]
)

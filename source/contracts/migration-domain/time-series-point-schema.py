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
            col("TransactionId"),
            col("Period.Resolution"),
            col("time"),
            col("year"),
            col("month"),
            col("day"),
"""


# Schema for time series point
# Data must be stored in a Delta table.
# The table must be partitioned by the observation time elements: year/month/date.
# New data must be appended.
time_series_point_schema = StructType(
    [
        # The metering point GSRN number that uniquely identifies the metering point
        StructField("MeteringPointId", StringType(), True),
        StructField("Quantity", DecimalType(18,6), True),
        # A02: Quantity missing (0-stilling)
        # A03: for estimated
        # A04: Measured
        # A05: for incomplete values
        StructField("Quality", StringType(), True),
        # The time when the energy was measured
        # Operation time (the time when the power was consumed/produced/exchanged).
        StructField("ObservationTime", TimestampType(), True),
        # The year part of the 'time' field.
        StructField("ObservationYear", IntegerType(), True),
        # The month part of the 'time' field.
        StructField("ObservationMonth", IntegerType(), True),
        # The day part of the 'time' field.
        StructField("ObservationDate", IntegerType(), True),
        # The registration time as indicated by the MDR actor
        # The registration date from the market document.
        StructField("RegistrationTime", TimestampType(), True),
    ]
)

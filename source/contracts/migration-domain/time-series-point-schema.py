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
"""


# Schema for time series points
# Data must be stored in a Delta table.
# The table must be partitioned by the observation time elements: year/month/date.
# New data must be appended.
time_series_point_schema = StructType(
    [
        # The GSRN that uniquely identifies the metering point
        StructField("MeteringPointId", StringType(), True),
        # Measured energy amount
        StructField("Quantity", DecimalType(18,6), True),
        # "A02": Quantity missing (reset)
        # "A03": for estimated
        # "A04": Measured
        # "A05": for incomplete values
        StructField("Quality", StringType(), True),
        # The time when the energy was measured
        # Operation time (the time when the power was consumed/produced/exchanged)
        StructField("ObservationTime", TimestampType(), True),
        # The year part of the `ObservationTime`. Used in partition.
        StructField("ObservationYear", IntegerType(), True),
        # The month part of the `ObservationTime`. Used in partition.
        StructField("ObservationMonth", IntegerType(), True),
        # The date part of the `ObservationTime`. Used in partition.
        StructField("ObservationDate", IntegerType(), True),
        # The registration time as indicated by the MDR actor
        StructField("RegistrationTime", TimestampType(), True),
    ]
)

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
Schema for time series points

Time series points are used in both balance fixing and settlement.

Data must be stored in a Delta table.
The table must be partitioned by the observation time elements: year/month/date.
New data must be appended.

TODO z-ordered desc by registration time?
"""
time_series_point_schema = StructType(
    [
        # The metering point GSRN number (18 characters) that uniquely identifies the metering point
        # Example: 578710000000000103
        StructField("MeteringPointId", StringType(), False),

        # Energy quantity for the given observation time.
        # Null when quality is missing.
        # Example: 1234.534217
        StructField("Quantity", DecimalType(18,6), True),

        # "A02" (missing) | "A03" (estimated) | "A04" (measured)
        # Example: A02
        StructField("Quality", StringType(), False),
        
        # The time when the energy was consumed/produced/exchanged
        StructField("ObservationTime", TimestampType(), False),
        
        # The year part of the `ObservationTime`. Used in partition.
        StructField("ObservationTime_Year", IntegerType(), False),
        
        # The month part of the `ObservationTime`. Used in partition.
        StructField("ObservationTime_Month", IntegerType(), False),
        
        # The date part of the `ObservationTime`. Used in partition.
        StructField("ObservationTime_Date", IntegerType(), False),
        
        # The registration time as specified by the MDR actor.
        # In DataHub 2 this is the creation date time.
        StructField("RegistrationTime", TimestampType(), False),
    ]
)

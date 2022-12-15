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
    DecimalType,
    IntegerType,
    StructField,
    StringType,
    TimestampType,
    StructType,
)
from package.constants import Colname

"""
Schema for time series points

Time series points are used in both balance fixing and settlement.

Data must be stored in a Delta table.
The table must be partitioned by the observation time elements: year/month/day.
Data must always be the current data.
"""
time_series_point_schema = StructType(
    [
        # GSRN (18 characters) that uniquely identifies the metering point
        # Example: 578710000000000103
        StructField(Colname.metering_point_id, StringType(), False),
        # Energy quantity for the given observation time.
        # Null when quality is missing.
        # Example: 1234.534217
        StructField(Colname.quantity, DecimalType(18, 6), True),
        # "A02" (missing) | "A03" (estimated) | "A04" (measured) | "A06" (calculated)
        # Example: A02
        StructField(Colname.quality, StringType(), False),
        # The time when the energy was consumed/produced/exchanged
        StructField(Colname.time, TimestampType(), False),
        # The year part of the `ObservationTime`. Used in partition.
        StructField(Colname.year, IntegerType(), False),
        # The month part of the `ObservationTime`. Used in partition.
        StructField(Colname.month, IntegerType(), False),
        # The day part (1-31) of the `ObservationTime`. Used in partition.
        StructField(Colname.day, IntegerType(), False),
    ]
)

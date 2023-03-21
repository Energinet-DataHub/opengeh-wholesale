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
Schema for time series points input data used by the calculator job.

Time series points are used in both balance fixing and settlement.

Data must be stored in a Delta table.
Data must always be the current data.
"""
time_series_point_schema = StructType(
    [
        # GSRN (18 characters) that uniquely identifies the metering point
        # Example: 578710000000000103
        StructField(Colname.metering_point_id, StringType(), False),
        # Energy quantity in kWh for the given observation time.
        # Null when quality is missing.
        # Example: 1234.534217
        StructField(Colname.quantity, DecimalType(18, 6), True),
        # "missing" | "estimated" | "measured" | "calculated"
        # Example: measured
        StructField(Colname.quality, StringType(), False),
        # The time when the energy was consumed/produced/exchanged
        StructField("ObservationTime", TimestampType(), False),
    ]
)

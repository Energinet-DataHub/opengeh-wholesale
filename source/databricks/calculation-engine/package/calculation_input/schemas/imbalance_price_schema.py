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
    StructField,
    StringType,
    TimestampType,
    StructType,
)

"""
Schema for imbalance price

Imbalance prices are only used for settlement.
Imbalance prices originates at NOIS.

Data must be stored in a Delta table.
The table must be partitioned by the observation time elements: year/month/date.
New data must be appended.
"""
imbalance_price_schema = StructType(
    [
        # Imbalance price value in actual currency per energy unit. In DataHub this is kr/kWh.
        # Example: 2.43806
        StructField("Price", DecimalType(18, 2), False),
        # Regarding the ”Imbalance price”, please note that the "Spot price" was used
        # for the period before the movement of the imbalance settlement to eSett (01.02.2021).
        # Value set: "spot" | "imbalance"
        # Example: spot
        StructField("Type", StringType(), False),
        # The time where the price applies in UTC.
        # Resolution is per hour.
        StructField("Time", TimestampType(), False),
        # The time when the imbalance price was received from NOIS.
        # TODO Only needed if we don't create snapshots
        StructField("ReceivedTime", TimestampType(), False),
    ]
)

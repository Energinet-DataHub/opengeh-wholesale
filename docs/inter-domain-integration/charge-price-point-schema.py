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
Schema for charge price points

Charge price points are only used in settlement.

Data must be stored in a Delta table.
The table must be partitioned by the observation time elements: year/month/day.
Data must always be the current data.
"""
charge_price_point_schema = StructType(
    [
        # ID of the charge
        # The ID is only guaranteed to be unique for a specific actor and charge type.
        # The ID is provided by the charge owner (actor).
        # Example: 0010643756
        StructField("SenderProvidedChargeId", StringType(), False),

        # "subscription" | "fee" | "tariff"
        # Example: subscription
        StructField("ChargeType", StringType(), False),

        # The unique GLN/EIC number of the charge owner (actor)
        # Example: 8100000000030
        StructField("ChargeOwnerId", StringType(), False),

        # The charge price. In the danish DataHub the price is in the DKK currency.
        # Example: 1234.53421700
        StructField("ChargePrice", DecimalType(18, 8), False),

        # The time when the energy was consumed/produced/exchanged
        StructField("ObservationTime", TimestampType(), False),
        
        # The year part of the `ObservationTime`. Used in partition.
        StructField("ObservationTime_Year", IntegerType(), False),
        
        # The month part of the `ObservationTime`. Used in partition.
        StructField("ObservationTime_Month", IntegerType(), False),
        
        # The day part (1-31) of the `ObservationTime`. Used in partition.
        StructField("ObservationTime_Day", IntegerType(), False)
    ]
)

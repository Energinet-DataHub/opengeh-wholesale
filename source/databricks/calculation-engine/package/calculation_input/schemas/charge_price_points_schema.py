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
Schema for charge price points

Charge price points are only used in settlement.

Data must be stored in a Delta table.
Data must always be the current data.
"""
charge_price_points_schema = StructType(
    [
        # The ID is only guaranteed to be unique for a specific actor and charge type.
        # The ID is provided by the charge owner (actor).
        # Example: 0010643756
        StructField("charge_code", StringType(), False),
        # "subscription" | "fee" | "tariff"
        # Example: subscription
        StructField("charge_type", StringType(), False),
        # The unique GLN/EIC number of the charge owner (actor)
        # Example: 8100000000030
        StructField("charge_owner_id", StringType(), False),
        # The charge price. In the danish DataHub the price is in the DKK currency.
        # Example: 1234.534217
        StructField("charge_price", DecimalType(18, 6), False),
        # The time where the price applies
        StructField("observation_time", TimestampType(), False),
    ]
)

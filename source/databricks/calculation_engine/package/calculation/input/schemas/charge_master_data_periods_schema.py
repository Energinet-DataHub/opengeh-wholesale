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
    BooleanType,
    StructField,
    StringType,
    TimestampType,
    StructType,
)

"""
Schema for charge master data.

Charge  master data is only used in settlement.

Data must be stored in a Delta table.

The table data must always contain current data.
"""
charge_master_data_periods_schema = StructType(
    [
        # ID of the charge
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
        # "PT1H" (hourly) | "P1D" (daily) | "P1M" (monthly)
        # Behaviour depends on the type of the charge.
        # - Subscriptions: Always monthly
        # - Fees: Always monthly. The value is charged on the effective day on the metering point
        # - Tariffs: Only hourly and daily resolution applies
        # Example: PT1H
        StructField("resolution", StringType(), False),
        # Specifies whether the charge is tax. Applies only to tariffs.
        # For subscriptions and fees the value must be false.
        # Example: True
        StructField("is_tax", BooleanType(), False),
        # The start date of the master data period. The start date must be the UTC time of the beginning of a date in the given timezone/DST.
        # The date is inclusive.
        StructField("from_date", TimestampType(), False),
        # The to-date of the master data period. The to-date must be the UTC time of the beginning of a date in the given timezone/DST.
        # The moment is exclusive.
        # All but the `to_date` of the last master data period must have value.
        # If the last master data period has a `to_date` value it means that the charge is stopped.
        StructField("to_date", TimestampType(), True),
    ]
)

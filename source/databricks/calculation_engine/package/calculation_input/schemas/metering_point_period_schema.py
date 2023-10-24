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
Schema for metering point periods input data used by the calculator job.

Metering point periods are used in balance fixing and settlement. Some fields are only used in settlement. See each field for details.
Periods (given by `FromDate` and `ToDate`) must not overlap and must not have gaps in between.

Only periods meeting the following requirements are included:
- metering point is connected (E22) or disconnected (E23)
- metering point consumption (E17) and production (E18) has energy supplier
- child metering points where parent has energy supplier

Data must be stored in a Delta table.
The table holds all consumption, production, exchange, and child metering points.

The table data must always contain updated periods.
"""
metering_point_period_schema = StructType(
    [
        # GSRN (18 characters) that uniquely identifies the metering point
        # Used in balance fixing and settlement.
        # Example: 578710000000000103
        StructField("metering_point_id", StringType(), False),
        # "E17" (consumption) | "E18" (production) | "E20" (exchange) | "D01", "D05"-"D12", "D14", "D15", "D19" (child)
        # Used in balance fixing and settlement. However, child metering points are only used in settlement.
        # Example: E20
        StructField("type", StringType(), False),
        # "system-correction" | "grid-loss"
        # For non-calculated metering points the calculation type is null.
        # Example: system-correction
        StructField("calculation_type", StringType(), True),
        # "E02" (non-profiled)| "D01" (flex)
        # When metering point is not a consumption (E17) metering point the value is null. Otherwise it must have a value.
        # Used in balance fixing and settlement.
        # Example: D01
        StructField("settlement_method", StringType(), True),
        # 3 character grid area code uniquely identifying the grid area. All characters must be digits (0-9).
        # Used in balance fixing and settlement.
        # Example: 805
        StructField("grid_area_code", StringType(), False),
        # "PT1H" (hourly) | "PT15M" (quarterly)
        # Used in balance fixing and settlement.
        # Example: PT1H
        StructField("resolution", StringType(), False),
        # 3 character grid area code uniquely identifying the from-grid area. All characters must be digits (0-9).
        # The code has a value for exchange (E20) metering points. For all other types it's null.
        # Used in balance fixing and settlement.
        # Example: 122
        StructField("from_grid_area_code", StringType(), True),
        # 3 character grid area code uniquely identifying the to-grid area. All characters must be digits (0-9).
        # The code has a value for E20 (exchange) metering points. For all other types it's null.
        # Used in balance fixing and settlement.
        # Example: 134
        StructField("to_grid_area_code", StringType(), True),
        # The id of the parent metering point or null.
        # Used only in settlement.
        # Example: 578710000000000112
        StructField("parent_metering_point_id", StringType(), True),
        # The unique GLN/EIC number of the energy supplier
        # There are two cases were the input table metering_point_period can have an energy_supplier_id of null:
        # 1. When the metering point is a child metering point
        # 2. When the metering point is an exchange (E20) metering point (not relevant for wholesale calculations)
        # Used in balance fixing and settlement.
        # Example: 8100000000108
        StructField("energy_supplier_id", StringType(), True),
        # The unique GLN/EIC number of the balance responsible
        # There are two cases were the input table metering_point_period can have an energy_supplier_id of null:
        # 1. When the metering point is a child metering point
        # 2. When the metering point is an exchange (E20) metering point (not relevant for wholesale calculations)
        # Used in balance fixing and settlement.
        # Example: 8100000000109
        StructField("balance_responsible_id", StringType(), True),
        # The start date of the period. The start date must be the UTC time of the beginning of a date in the given timezone/DST.
        # The date is inclusive.
        # Used in balance fixing and settlement.
        StructField("from_date", TimestampType(), False),
        # The to-date of the period. The to-date must be the UTC time of the beginning of a date in the given timezone/DST.
        # The moment is exclusive.
        # The date of the last period is null when the metering point has not been closed down.
        # Otherwise it is the date where the metering point was closed down.
        # Used in balance fixing and settlement.
        StructField("to_date", TimestampType(), True),
    ]
)

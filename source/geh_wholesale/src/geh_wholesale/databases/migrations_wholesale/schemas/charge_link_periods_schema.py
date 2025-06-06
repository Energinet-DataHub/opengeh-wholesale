from pyspark.sql.types import (
    IntegerType,
    StringType,
    StructField,
    StructType,
    TimestampType,
)

"""
Schema for charge link periods (and charges)

Charge link periods are only used in settlement.
Periods (given by `FromDate` and `ToDate`) must not overlap but may have gaps.
Gaps may occur if the link has been removed for a period before being added again.

Data must be stored in a Delta table.

It is important to partition by to-date instead of from-date as it will ensure efficient data filtering.
This is because most periods will have a to-date prior to the calculation period start date.

The table data must always contain updated periods.
"""
charge_link_periods_schema = StructType(
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
        # GSRN (18 characters) that uniquely identifies the metering point
        # The field is from the charge link.
        # Example: 578710000000000103
        StructField("metering_point_id", StringType(), False),
        # Quantity (also known as factor)
        # Value is 1 or larger. For tariffs it's always 1.
        # The field is from the charge link.
        StructField("quantity", IntegerType(), False),
        # The start date of the link period. The start date must be the UTC time of the beginning of a date in the given timezone/DST.
        # The date is inclusive.
        StructField("from_date", TimestampType(), False),
        # The to-date of the link period. The to-date must be the UTC time of the beginning of a date in the given timezone/DST.
        # The moment is exclusive.
        # All but the `to_date` of the last period must have value. The `to_date` of the last period can be null for subscriptions and tariffs.
        # The `to_date` of fees is the day after the `from_date`.
        StructField("to_date", TimestampType(), True),
    ]
)

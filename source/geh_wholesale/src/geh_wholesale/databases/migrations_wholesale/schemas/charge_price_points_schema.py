from pyspark.sql.types import (
    DecimalType,
    StringType,
    StructField,
    StructType,
    TimestampType,
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
        StructField("charge_time", TimestampType(), False),
    ]
)

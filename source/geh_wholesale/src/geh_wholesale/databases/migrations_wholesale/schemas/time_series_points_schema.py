from pyspark.sql.types import (
    DecimalType,
    StringType,
    StructField,
    StructType,
    TimestampType,
)

"""
Schema for time series points input data used by the calculator job.

Time series points are used in both balance fixing and settlement.

Data must be stored in a Delta table.
Data must always be the current data.
"""
time_series_points_schema = StructType(
    [
        # GSRN (18 characters) that uniquely identifies the metering point
        # Example: 578710000000000103
        StructField("metering_point_id", StringType(), False),

        # Energy quantity in kWh for the given observation time.
        # Null when quality is missing.
        # Example: 1234.534217
        StructField("quantity", DecimalType(18, 3), True),

        # "missing" | "estimated" | "measured" | "calculated"
        # Example: measured
        StructField("quality", StringType(), False),

        # The time when the energy was consumed/produced/exchanged
        StructField("observation_time", TimestampType(), False),
    ]
)

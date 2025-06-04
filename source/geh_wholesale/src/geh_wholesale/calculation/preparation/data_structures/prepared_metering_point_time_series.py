import pyspark.sql.types as t
from geh_common.pyspark.data_frame_wrapper import DataFrameWrapper
from pyspark.sql import DataFrame

from geh_wholesale.constants import Colname


class PreparedMeteringPointTimeSeries(DataFrameWrapper):
    """Represents time series enriched with metering points master data.

    The time series are prepared for calculation.
    """

    def __init__(self, df: DataFrame):
        super().__init__(
            df,
            prepared_metering_point_time_series_schema,
            # We ignore_nullability because it has turned out to be too hard and even possibly
            # introducing more errors than solving in order to stay in exact sync with the
            # logically correct schema.
            ignore_nullability=True,
            ignore_decimal_scale=True,
            ignore_decimal_precision=True,
        )


# The nullability and decimal types are not precisely representative of the actual data frame schema at runtime,
# See comments to the `assert_schema()` invocation.
prepared_metering_point_time_series_schema = t.StructType(
    [
        t.StructField(Colname.grid_area_code, t.StringType(), False),
        t.StructField(Colname.to_grid_area_code, t.StringType(), True),
        t.StructField(Colname.from_grid_area_code, t.StringType(), True),
        t.StructField(Colname.metering_point_id, t.StringType(), False),
        t.StructField(Colname.metering_point_type, t.StringType(), False),
        t.StructField(Colname.resolution, t.StringType(), False),
        t.StructField(Colname.observation_time, t.TimestampType(), False),
        t.StructField(Colname.quantity, t.DecimalType(18, 6), False),
        t.StructField(Colname.quality, t.StringType(), False),
        t.StructField(Colname.energy_supplier_id, t.StringType(), True),
        t.StructField(Colname.balance_responsible_party_id, t.StringType(), True),
        t.StructField(Colname.settlement_method, t.StringType(), True),
    ]
)

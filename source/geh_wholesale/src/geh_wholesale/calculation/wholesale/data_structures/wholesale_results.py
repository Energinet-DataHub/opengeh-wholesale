import pyspark.sql.types as t
from geh_common.pyspark.data_frame_wrapper import DataFrameWrapper
from pyspark.sql import DataFrame

from geh_wholesale.constants import Colname


class WholesaleResults(DataFrameWrapper):
    """Time series of energy results.

    Only exchange energy results have to- and from- grid area values.
    """

    def __init__(self, df: DataFrame):
        """Fit data frame in a general DataFrame. This is used for all results and missing columns will be null."""
        super().__init__(
            df,
            wholesale_results_schema,
            # We ignore_nullability because it has turned out to be too hard and even possibly
            # introducing more errors than solving in order to stay in exact sync with the
            # logically correct schema.
            ignore_nullability=True,
            ignore_decimal_scale=True,
            ignore_decimal_precision=True,
        )


# The nullability and decimal types are not precisely representative of the actual data frame schema at runtime,
# See comments to the `assert_schema()` invocation.
wholesale_results_schema = t.StructType(
    [
        t.StructField(Colname.grid_area_code, t.StringType(), False),
        t.StructField(Colname.energy_supplier_id, t.StringType(), False),
        t.StructField(Colname.total_quantity, t.DecimalType(18, 3), True),
        t.StructField(Colname.unit, t.StringType(), False),
        t.StructField(Colname.qualities, t.ArrayType(t.StringType(), True), True),
        t.StructField(Colname.charge_time, t.TimestampType(), False),
        t.StructField(Colname.resolution, t.StringType(), False),
        t.StructField(Colname.metering_point_type, t.StringType(), True),
        t.StructField(Colname.settlement_method, t.StringType(), True),
        t.StructField(Colname.charge_price, t.DecimalType(18, 6), True),
        t.StructField(Colname.total_amount, t.DecimalType(18, 6), True),
        t.StructField(Colname.charge_tax, t.BooleanType(), False),
        t.StructField(Colname.charge_code, t.StringType(), False),
        t.StructField(Colname.charge_type, t.StringType(), False),
        t.StructField(Colname.charge_owner, t.StringType(), False),
    ]
)

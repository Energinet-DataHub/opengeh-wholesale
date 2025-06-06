import pyspark.sql.types as t
from geh_common.pyspark.data_frame_wrapper import DataFrameWrapper
from pyspark.sql import DataFrame

from geh_wholesale.constants import Colname


class EnergyResults(DataFrameWrapper):
    """Time series of energy results.

    See the schema comments for details on nullable columns.
    """

    def __init__(self, df: DataFrame):
        """Fit data frame in a general DataFrame. This is used for all results and missing columns will be null."""
        super().__init__(
            df,
            energy_results_schema,
            # We ignore_nullability because it has turned out to be too hard and even possibly
            # introducing more errors than solving in order to stay in exact sync with the
            # logically correct schema.
            ignore_nullability=True,
            ignore_decimal_scale=False,
            ignore_decimal_precision=True,
        )


# The nullability and decimal types are not precisely representative of the actual data frame schema at runtime,
# See comments to the `assert_schema()` invocation.
energy_results_schema = t.StructType(
    [
        t.StructField(Colname.grid_area_code, t.StringType(), False),
        # Required for exchange, otherwise null
        t.StructField(Colname.to_grid_area_code, t.StringType(), True),
        # Required for exchange, otherwise null
        t.StructField(Colname.from_grid_area_code, t.StringType(), True),
        # Required for non-exchange when aggregated per es or brp, otherwise null
        t.StructField(Colname.balance_responsible_party_id, t.StringType(), True),
        # Required when aggregated per es, otherwise null
        t.StructField(Colname.energy_supplier_id, t.StringType(), True),
        t.StructField(Colname.observation_time, t.TimestampType(), False),
        t.StructField(Colname.quantity, t.DecimalType(18, 3), False),
        # Grid loss has only a single quality (calculated)
        t.StructField(Colname.qualities, t.ArrayType(t.StringType(), False), False),
        # Requires for grid loss, otherwise null
        t.StructField(Colname.metering_point_id, t.StringType(), True),
    ]
)

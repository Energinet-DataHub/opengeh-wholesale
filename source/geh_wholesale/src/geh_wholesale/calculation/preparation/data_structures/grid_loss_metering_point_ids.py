import pyspark.sql.types as t
from geh_common.pyspark.data_frame_wrapper import DataFrameWrapper
from pyspark.sql import DataFrame

from geh_wholesale.constants import Colname


class GridLossMeteringPointIds(DataFrameWrapper):
    """Represents grid loss metering points."""

    def __init__(self, df: DataFrame):
        super().__init__(
            df,
            grid_loss_metering_point_ids_schema,
        )


# The nullability and decimal types are not precisely representative of the actual data frame schema at runtime,
# See comments to the `assert_schema()` invocation.
grid_loss_metering_point_ids_schema = t.StructType(
    [
        t.StructField(Colname.metering_point_id, t.StringType(), False),
    ]
)

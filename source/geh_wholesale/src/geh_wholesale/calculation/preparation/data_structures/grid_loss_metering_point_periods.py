import pyspark.sql.types as t
from geh_common.pyspark.data_frame_wrapper import DataFrameWrapper
from pyspark.sql import DataFrame

from geh_wholesale.constants import Colname


class GridLossMeteringPointPeriods(DataFrameWrapper):
    """Grid loss metering point periods including energy supplier and balance responsible party."""

    def __init__(self, df: DataFrame):
        super().__init__(
            df,
            grid_loss_metering_point_periods_schema,
            ignore_nullability=True,
        )


grid_loss_metering_point_periods_schema = t.StructType(
    [
        t.StructField(Colname.metering_point_id, t.StringType(), False),
        t.StructField(Colname.grid_area_code, t.StringType(), False),
        t.StructField(Colname.from_date, t.TimestampType(), False),
        t.StructField(Colname.to_date, t.TimestampType(), True),
        t.StructField(Colname.metering_point_type, t.StringType(), False),
        t.StructField(Colname.energy_supplier_id, t.StringType(), False),
        t.StructField(Colname.balance_responsible_party_id, t.StringType(), False),
    ]
)

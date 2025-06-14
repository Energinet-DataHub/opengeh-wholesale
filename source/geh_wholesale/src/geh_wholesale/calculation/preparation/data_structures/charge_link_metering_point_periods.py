import pyspark.sql.types as t
from geh_common.pyspark.data_frame_wrapper import DataFrameWrapper
from pyspark.sql import DataFrame

from geh_wholesale.codelists import ChargeType
from geh_wholesale.constants import Colname


class ChargeLinkMeteringPointPeriods(DataFrameWrapper):
    """Represents the metering point period enriched with information on the relation to a charge.

    The relation is represented by the charge key and the charge quantity.
    All periods are clamped to least common period of the metering point and the charge link.
    Metering points that have no charge links in the calculation period are not included.
    """

    def __init__(self, df: DataFrame):
        super().__init__(
            df,
            charge_link_metering_point_periods_schema,
            # We ignore_nullability because it has turned out to be too hard and even possibly
            # introducing more errors than solving in order to stay in exact sync with the
            # logically correct schema.
            ignore_nullability=True,
            ignore_decimal_scale=True,
            ignore_decimal_precision=True,
        )

    def filter_by_charge_type(self, charge_type: ChargeType) -> "ChargeLinkMeteringPointPeriods":
        df = self._df.filter(self._df[Colname.charge_type] == charge_type.value)
        return ChargeLinkMeteringPointPeriods(df)


# The nullability and decimal types are not precisely representative of the actual data frame schema at runtime,
# See comments to the `assert_schema()` invocation.
charge_link_metering_point_periods_schema = t.StructType(
    [
        t.StructField(Colname.charge_key, t.StringType(), False),
        t.StructField(Colname.charge_type, t.StringType(), False),
        t.StructField(Colname.metering_point_id, t.StringType(), False),
        t.StructField(Colname.quantity, t.IntegerType(), False),
        t.StructField(Colname.from_date, t.TimestampType(), True),
        t.StructField(Colname.to_date, t.TimestampType(), True),
        t.StructField(Colname.metering_point_type, t.StringType(), False),
        t.StructField(Colname.settlement_method, t.StringType(), True),
        t.StructField(Colname.grid_area_code, t.StringType(), False),
        t.StructField(Colname.energy_supplier_id, t.StringType(), False),
    ]
)

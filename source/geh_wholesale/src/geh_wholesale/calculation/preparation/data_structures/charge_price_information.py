import pyspark.sql.types as t
from geh_common.pyspark.data_frame_wrapper import DataFrameWrapper
from pyspark.sql import DataFrame

from geh_wholesale.codelists import ChargeType
from geh_wholesale.constants import Colname


class ChargePriceInformation(DataFrameWrapper):
    """Represents the charge master data.

    All periods are clamped to least common period of the metering point and the charge master data period.
    """

    def __init__(self, df: DataFrame):
        super().__init__(
            df,
            charge_price_information_schema,
            # We ignore_nullability because it has turned out to be too hard and even possibly
            # introducing more errors than solving in order to stay in exact sync with the
            # logically correct schema.
            ignore_nullability=True,
            ignore_decimal_scale=True,
            ignore_decimal_precision=True,
        )

    def filter_by_charge_type(self, charge_type: ChargeType) -> "ChargePriceInformation":
        df = self._df.filter(self._df[Colname.charge_type] == charge_type.value)
        return ChargePriceInformation(df)


# The nullability and decimal types are not precisely representative of the actual data frame schema at runtime,
# See comments to the `assert_schema()` invocation.
charge_price_information_schema = t.StructType(
    [
        t.StructField(Colname.charge_key, t.StringType(), False),
        t.StructField(Colname.charge_code, t.StringType(), False),
        t.StructField(Colname.charge_type, t.StringType(), False),
        t.StructField(Colname.charge_owner, t.StringType(), False),
        t.StructField(Colname.charge_tax, t.BooleanType(), False),
        t.StructField(Colname.resolution, t.StringType(), False),
        t.StructField(Colname.from_date, t.TimestampType(), True),
        t.StructField(Colname.to_date, t.TimestampType(), True),
    ]
)

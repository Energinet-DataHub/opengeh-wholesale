from datetime import datetime

import pyspark.sql.functions as f
from pyspark.sql.types import ArrayType, DecimalType, StringType

from geh_wholesale.calculation.wholesale.data_structures import MonthlyAmountPerCharge
from geh_wholesale.calculation.wholesale.data_structures.wholesale_results import (
    WholesaleResults,
)
from geh_wholesale.codelists import WholesaleResultResolution
from geh_wholesale.constants import Colname


def sum_within_month(
    wholesale_results: WholesaleResults,
    period_start_datetime: datetime,
) -> MonthlyAmountPerCharge:
    agg_df = (
        wholesale_results.df.groupBy(
            Colname.energy_supplier_id,
            Colname.grid_area_code,
            Colname.charge_code,
            Colname.charge_type,
            Colname.charge_owner,
        )
        .agg(
            f.sum(Colname.total_amount).alias(Colname.total_amount),
            # charge_tax is the same for all tariffs in a given month
            f.first(Colname.charge_tax).alias(Colname.charge_tax),
            # tariff unit is the same for all tariffs in a given month (kWh)
            f.first(Colname.unit).alias(Colname.unit),
        )
        .select(
            f.col(Colname.grid_area_code),
            f.col(Colname.energy_supplier_id),
            f.lit(None).cast(DecimalType(18, 3)).alias(Colname.total_quantity),
            f.col(Colname.unit),
            f.lit(None).cast(ArrayType(StringType())).alias(Colname.qualities),
            f.lit(period_start_datetime).alias(Colname.charge_time),
            f.lit(WholesaleResultResolution.MONTH.value).alias(Colname.resolution),
            f.lit(None).cast(StringType()).alias(Colname.metering_point_type),
            f.lit(None).cast(StringType()).alias(Colname.settlement_method),
            f.lit(None).cast(DecimalType(18, 6)).alias(Colname.charge_price),
            f.col(Colname.total_amount),
            f.col(Colname.charge_tax),
            f.col(Colname.charge_code),
            f.col(Colname.charge_type),
            f.col(Colname.charge_owner),
        )
    )

    return MonthlyAmountPerCharge(agg_df)

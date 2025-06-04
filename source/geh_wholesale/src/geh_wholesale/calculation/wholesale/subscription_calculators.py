from datetime import datetime

import pyspark.sql.functions as f
from pyspark.sql import DataFrame

import geh_wholesale.common.datetime_utils as datetime_utils
from geh_wholesale.calculation.preparation.data_structures.prepared_subscriptions import (
    PreparedSubscriptions,
)
from geh_wholesale.calculation.wholesale.calculate_total_quantity_and_amount import (
    calculate_total_quantity_and_amount,
)
from geh_wholesale.calculation.wholesale.data_structures.wholesale_results import (
    WholesaleResults,
)
from geh_wholesale.codelists import ChargeType
from geh_wholesale.constants import Colname


def calculate(
    prepared_subscriptions: PreparedSubscriptions,
    calculation_period_start: datetime,
    calculation_period_end: datetime,
    time_zone: str,
) -> WholesaleResults:
    prepared_subscriptions_df = prepared_subscriptions.df
    subscriptions_with_daily_price = _calculate_price_per_day(
        prepared_subscriptions_df,
        calculation_period_start,
        calculation_period_end,
        time_zone,
    )

    subscription_amount_per_charge = calculate_total_quantity_and_amount(
        subscriptions_with_daily_price, charge_type=ChargeType.SUBSCRIPTION
    )

    return WholesaleResults(subscription_amount_per_charge)


def _calculate_price_per_day(
    prepared_subscriptions: DataFrame,
    calculation_period_start: datetime,
    calculation_period_end: datetime,
    time_zone: str,
) -> DataFrame:
    days_in_period = datetime_utils.get_number_of_days_in_period(
        calculation_period_start, calculation_period_end, time_zone
    )

    subscriptions_with_daily_price = prepared_subscriptions.withColumn(
        Colname.charge_price,
        f.round((f.col(Colname.charge_price) / f.lit(days_in_period)), 6),
    )

    return subscriptions_with_daily_price

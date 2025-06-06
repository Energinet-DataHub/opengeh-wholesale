import pyspark.sql.functions as f
from pyspark.sql import DataFrame

from geh_wholesale.constants import Colname


def aggregate_quantity_and_quality(result: DataFrame, group_by: list[str]) -> DataFrame:
    """Aggregate values from metering point time series and groups into an aggregated time-series.

    Sums quantity and collects distinct quality from the metering point time-series.
    """
    return result.groupBy(group_by).agg(
        f.sum(Colname.quantity).alias(Colname.quantity),
        f.collect_set(Colname.quality).alias(Colname.qualities),
    )


def aggregate_sum_quantity_and_qualities(result: DataFrame, group_by: list[str]) -> DataFrame:
    """Aggregate values from already aggregated time series (energy results) and groups into further aggregated time-series (also energy results).

    Sums sum_quantity and collects distinct qualities from the aggregated time-series.
    """
    return result.groupBy(group_by).agg(
        f.sum(Colname.quantity).alias(Colname.quantity),
        f.array_distinct(f.flatten(f.collect_set(Colname.qualities))).alias(Colname.qualities),
    )

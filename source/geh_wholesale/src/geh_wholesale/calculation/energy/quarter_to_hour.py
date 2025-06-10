import pyspark.sql.functions as f

from geh_wholesale.calculation.energy.aggregators.transformations import (
    aggregate_quantity_and_quality,
)
from geh_wholesale.calculation.preparation.data_structures.metering_point_time_series import (
    MeteringPointTimeSeries,
)
from geh_wholesale.calculation.preparation.data_structures.prepared_metering_point_time_series import (
    PreparedMeteringPointTimeSeries,
)
from geh_wholesale.codelists import QuantityQuality
from geh_wholesale.constants import Colname


def transform_quarter_to_hour(
    metering_point_time_series: PreparedMeteringPointTimeSeries,
) -> MeteringPointTimeSeries:
    df = metering_point_time_series.df
    result = df.withColumn(Colname.observation_time, f.date_trunc("hour", Colname.observation_time))
    group_by = [col for col in df.columns if col != Colname.quantity and col != Colname.quality]
    result = aggregate_quantity_and_quality(result, group_by)

    result = result.withColumn(
        Colname.quality,
        f.when(
            f.array_contains(f.col(Colname.qualities), QuantityQuality.CALCULATED.value),
            QuantityQuality.CALCULATED.value,
        )
        .when(
            f.array_contains(f.col(Colname.qualities), QuantityQuality.ESTIMATED.value),
            QuantityQuality.ESTIMATED.value,
        )
        .when(
            (
                f.array_contains(
                    f.col(Colname.qualities),
                    QuantityQuality.MEASURED.value,
                )
                & f.array_contains(f.col(Colname.qualities), QuantityQuality.MISSING.value)
            ),
            QuantityQuality.ESTIMATED.value,
        )
        .when(
            f.array_contains(f.col(Colname.qualities), QuantityQuality.MEASURED.value),
            QuantityQuality.MEASURED.value,
        )
        .otherwise(QuantityQuality.MISSING.value),
    )

    return MeteringPointTimeSeries(result)

import pyspark.sql.functions as f

from geh_wholesale.calculation.preparation.data_structures.metering_point_time_series import (
    MeteringPointTimeSeries,
)
from geh_wholesale.calculation.preparation.data_structures.prepared_metering_point_time_series import (
    PreparedMeteringPointTimeSeries,
)
from geh_wholesale.codelists import MeteringPointResolution
from geh_wholesale.constants import Colname


def transform_hour_to_quarter(
    metering_point_time_series: PreparedMeteringPointTimeSeries,
) -> MeteringPointTimeSeries:
    result = metering_point_time_series.df.withColumn(
        "quarter_times",
        f.when(
            f.col(Colname.resolution) == MeteringPointResolution.HOUR.value,
            f.array(
                f.col(Colname.observation_time),
                f.col(Colname.observation_time) + f.expr("INTERVAL 15 minutes"),
                f.col(Colname.observation_time) + f.expr("INTERVAL 30 minutes"),
                f.col(Colname.observation_time) + f.expr("INTERVAL 45 minutes"),
            ),
        ).when(
            f.col(Colname.resolution) == MeteringPointResolution.QUARTER.value,
            f.array(f.col(Colname.observation_time)),
        ),
    ).select(
        metering_point_time_series.df["*"],
        f.explode("quarter_times").alias("quarter_time"),
    )

    result = result.withColumn(
        Colname.observation_time,
        f.col("quarter_time"),
    )

    result = result.withColumn(
        Colname.quantity,
        f.coalesce(
            f.when(
                f.col(Colname.resolution) == MeteringPointResolution.HOUR.value,
                f.col(Colname.quantity) / 4,
            ),
            # When resolution is quarter, quantity is already correct
            f.col(Colname.quantity),
        ),
    )

    return MeteringPointTimeSeries(result)

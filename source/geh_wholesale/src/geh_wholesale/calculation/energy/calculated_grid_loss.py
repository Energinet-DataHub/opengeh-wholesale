import pyspark.sql.functions as f

from geh_wholesale.calculation.calculator_args import CalculatorArgs
from geh_wholesale.calculation.energy.data_structures.energy_results import (
    EnergyResults,
)
from geh_wholesale.calculation.energy.resolution_transition_factory import (
    get_energy_result_resolution,
)
from geh_wholesale.calculation.preparation.data_structures.prepared_metering_point_time_series import (
    PreparedMeteringPointTimeSeries,
)
from geh_wholesale.codelists import (
    MeteringPointType,
    QuantityQuality,
    SettlementMethod,
)
from geh_wholesale.constants import Colname


def append_calculated_grid_loss_to_metering_point_times_series(
    args: CalculatorArgs,
    metering_point_time_series: PreparedMeteringPointTimeSeries,
    positive_grid_loss: EnergyResults,
    negative_grid_loss: EnergyResults,
) -> PreparedMeteringPointTimeSeries:
    """Append grid loss metering point time series to the calculation input metering point time series.

    Metering point time series for wholesale calculation includes all calculation input metering point time series,
    and positive and negative grid loss metering point time series.
    """
    # Union positive and negative grid loss metering point time series and transform them to the same format as the
    # calculation input metering point time series before final union.
    positive = positive_grid_loss.df.withColumn(
        Colname.metering_point_type, f.lit(MeteringPointType.CONSUMPTION.value)
    ).withColumn(Colname.settlement_method, f.lit(SettlementMethod.FLEX.value))

    negative = negative_grid_loss.df.withColumn(
        Colname.metering_point_type, f.lit(MeteringPointType.PRODUCTION.value)
    ).withColumn(Colname.settlement_method, f.lit(None))

    df = (
        positive.union(negative)
        .select(
            f.col(Colname.grid_area_code),
            f.col(Colname.to_grid_area_code),
            f.col(Colname.from_grid_area_code),
            f.col(Colname.metering_point_id),
            f.col(Colname.metering_point_type),
            f.lit(
                get_energy_result_resolution(
                    args.quarterly_resolution_transition_datetime,
                    args.period_end_datetime,
                ).value
            ).alias(Colname.resolution),
            f.col(Colname.observation_time),
            f.col(Colname.quantity).alias(Colname.quantity),
            f.lit(QuantityQuality.CALCULATED.value).alias(Colname.quality),
            f.col(Colname.energy_supplier_id),
            f.col(Colname.balance_responsible_party_id),
            f.col(Colname.settlement_method),
        )
        .union(metering_point_time_series.df)
    )

    return PreparedMeteringPointTimeSeries(df)

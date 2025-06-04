from datetime import datetime

from geh_wholesale.calculation.calculator_args import CalculatorArgs
from geh_wholesale.calculation.energy.hour_to_quarter import transform_hour_to_quarter
from geh_wholesale.calculation.energy.quarter_to_hour import transform_quarter_to_hour
from geh_wholesale.calculation.preparation.data_structures import (
    PreparedMeteringPointTimeSeries,
)
from geh_wholesale.calculation.preparation.data_structures.metering_point_time_series import (
    MeteringPointTimeSeries,
)
from geh_wholesale.codelists import MeteringPointResolution


def get_energy_result_resolution(
    quarterly_resolution_transition_datetime: datetime,
    calculation_period_end_datetime: datetime,
) -> MeteringPointResolution:
    if calculation_period_end_datetime <= quarterly_resolution_transition_datetime:
        return MeteringPointResolution.HOUR
    return MeteringPointResolution.QUARTER


def get_energy_result_resolution_adjusted_metering_point_time_series(
    args: CalculatorArgs,
    prepared_metering_point_time_series: PreparedMeteringPointTimeSeries,
) -> MeteringPointTimeSeries:
    if (
        get_energy_result_resolution(
            args.quarterly_resolution_transition_datetime,
            args.period_end_datetime,
        )
        == MeteringPointResolution.HOUR
    ):
        return transform_quarter_to_hour(prepared_metering_point_time_series)
    else:
        return transform_hour_to_quarter(prepared_metering_point_time_series)

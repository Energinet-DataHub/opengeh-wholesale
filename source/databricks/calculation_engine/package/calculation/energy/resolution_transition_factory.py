# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
from datetime import datetime

from package.calculation.calculator_args import CalculatorArgs
from package.calculation.energy.hour_to_quarter import transform_hour_to_quarter
from package.calculation.energy.quarter_to_hour import transform_quarter_to_hour
from package.calculation.preparation.data_structures import (
    PreparedMeteringPointTimeSeries,
)
from package.calculation.preparation.data_structures.metering_point_time_series import (
    MeteringPointTimeSeries,
)
from package.codelists import MeteringPointResolution


def get_resolution(
    quarterly_resolution_transition_datetime: datetime,
    calculation_period_end_datetime: datetime,
) -> MeteringPointResolution:
    if calculation_period_end_datetime < quarterly_resolution_transition_datetime:
        return MeteringPointResolution.HOUR
    return MeteringPointResolution.QUARTER


def get_resolution_adjusted_metering_point_time_series(
    args: CalculatorArgs,
    prepared_metering_point_time_series: PreparedMeteringPointTimeSeries,
) -> MeteringPointTimeSeries:
    if (
        get_resolution(
            args.quarterly_resolution_transition_datetime,
            args.calculation_period_end_datetime,
        )
        == MeteringPointResolution.HOUR
    ):
        return transform_quarter_to_hour(prepared_metering_point_time_series)
    else:
        return transform_hour_to_quarter(prepared_metering_point_time_series)

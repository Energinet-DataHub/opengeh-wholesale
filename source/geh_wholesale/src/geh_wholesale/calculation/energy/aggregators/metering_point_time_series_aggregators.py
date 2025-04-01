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

import pyspark.sql.functions as f

from geh_wholesale.calculation.energy.aggregators.transformations.aggregate_sum_and_quality import (
    aggregate_quantity_and_quality,
)
from geh_wholesale.calculation.energy.data_structures.energy_results import (
    EnergyResults,
)
from geh_wholesale.calculation.preparation.data_structures.metering_point_time_series import (
    MeteringPointTimeSeries,
)
from geh_wholesale.calculation.preparation.transformations.rounding import (
    round_quantity,
)
from geh_wholesale.codelists import (
    MeteringPointType,
    SettlementMethod,
)
from geh_wholesale.constants import Colname


def aggregate_per_es(
    metering_point_time_series: MeteringPointTimeSeries,
    metering_point_type: MeteringPointType,
    settlement_method: SettlementMethod | None,
) -> EnergyResults:
    """Create an intermediate energy result, which is subsequently used to aggregate other energy results.

    The function is responsible for
    - Sum quantities across metering points per grid area, energy supplier, and balance responsible.
    - Assign quality when performing sum.
    - Filter by metering point type
    - Filter by settlement method (consumption only).

    Each row in the output dataframe corresponds to a unique combination of: ga, brp, es, and quarter_time
    """
    result = metering_point_time_series.df.where(f.col(Colname.metering_point_type) == metering_point_type.value)

    if settlement_method is not None:
        result = result.where(f.col(Colname.settlement_method) == settlement_method.value)

    sum_group_by = [
        Colname.grid_area_code,
        Colname.balance_responsible_party_id,
        Colname.energy_supplier_id,
        Colname.observation_time,
    ]
    result = aggregate_quantity_and_quality(result, sum_group_by)
    result = round_quantity(result)
    return EnergyResults(result)


def aggregate_non_profiled_consumption_per_es(
    metering_point_time_series: MeteringPointTimeSeries,
) -> EnergyResults:
    return aggregate_per_es(
        metering_point_time_series,
        MeteringPointType.CONSUMPTION,
        SettlementMethod.NON_PROFILED,
    )


def aggregate_flex_consumption_per_es(
    metering_point_time_series: MeteringPointTimeSeries,
) -> EnergyResults:
    return aggregate_per_es(
        metering_point_time_series,
        MeteringPointType.CONSUMPTION,
        SettlementMethod.FLEX,
    )


def aggregate_production_per_es(
    metering_point_time_series: MeteringPointTimeSeries,
) -> EnergyResults:
    return aggregate_per_es(metering_point_time_series, MeteringPointType.PRODUCTION, None)

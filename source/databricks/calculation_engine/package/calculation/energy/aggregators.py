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

from package.codelists import (
    MeteringPointType,
    SettlementMethod,
)
from package.constants import Colname
from . import transformations as t
from pyspark.sql.functions import (
    col,
    lit,
)
from typing import Union

from package.calculation.energy.energy_results import EnergyResults
from ..preparation.quarterly_metering_point_time_series import (
    QuarterlyMeteringPointTimeSeries,
)


def aggregate_non_profiled_consumption_ga_brp_es(
    quarterly_metering_point_time_series: QuarterlyMeteringPointTimeSeries,
) -> EnergyResults:
    return _aggregate_per_ga_and_brp_and_es(
        quarterly_metering_point_time_series,
        MeteringPointType.CONSUMPTION,
        SettlementMethod.NON_PROFILED,
    )


def aggregate_flex_consumption_ga_brp_es(
    quarterly_metering_point_time_series: QuarterlyMeteringPointTimeSeries,
) -> EnergyResults:
    return _aggregate_per_ga_and_brp_and_es(
        quarterly_metering_point_time_series,
        MeteringPointType.CONSUMPTION,
        SettlementMethod.FLEX,
    )


def aggregate_production_ga_brp_es(
    quarterly_metering_point_time_series: QuarterlyMeteringPointTimeSeries,
) -> EnergyResults:
    return _aggregate_per_ga_and_brp_and_es(
        quarterly_metering_point_time_series, MeteringPointType.PRODUCTION, None
    )


def _aggregate_per_ga_and_brp_and_es(
    quarterly_metering_point_time_series: QuarterlyMeteringPointTimeSeries,
    market_evaluation_point_type: MeteringPointType,
    settlement_method: Union[SettlementMethod, None],
) -> EnergyResults:
    """
    This function creates an intermediate energy result, which is subsequently used
    to aggregate other energy results.

    The function is responsible for
    - Sum quantities across metering points per grid area, energy supplier, and balance responsible.
    - Assign quality when performing sum.

    Each row in the output dataframe corresponds to a unique combination of: ga, brp, es, and quarter_time
    """

    result = quarterly_metering_point_time_series.df.where(
        col(Colname.metering_point_type) == market_evaluation_point_type.value
    )

    if settlement_method is not None:
        result = result.where(col(Colname.settlement_method) == settlement_method.value)

    sum_group_by = [
        Colname.grid_area,
        Colname.balance_responsible_id,
        Colname.energy_supplier_id,
        Colname.time_window,
        # Add the following columns to preserve them during the grouping
        Colname.metering_point_type,
        Colname.settlement_method,
    ]
    result = t.aggregate_quantity_and_quality(result, sum_group_by)

    return EnergyResults(result, True)


def aggregate_production_ga_es(production: EnergyResults) -> EnergyResults:
    return _aggregate_per_ga_and_es(
        production,
        MeteringPointType.PRODUCTION,
    )


def aggregate_non_profiled_consumption_ga_es(
    non_profiled_consumption: EnergyResults,
) -> EnergyResults:
    return _aggregate_per_ga_and_es(
        non_profiled_consumption,
        MeteringPointType.CONSUMPTION,
    )


def aggregate_flex_consumption_ga_es(flex_consumption: EnergyResults) -> EnergyResults:
    return _aggregate_per_ga_and_es(
        flex_consumption,
        MeteringPointType.CONSUMPTION,
    )


def _aggregate_per_ga_and_es(
    df: EnergyResults, market_evaluation_point_type: MeteringPointType
) -> EnergyResults:
    group_by = [Colname.grid_area, Colname.energy_supplier_id, Colname.time_window]
    result = t.aggregate_sum_quantity_and_qualities(df.df, group_by)

    result = result.withColumn(
        Colname.metering_point_type, lit(market_evaluation_point_type.value)
    )

    return EnergyResults(result)


def aggregate_production_ga_brp(production: EnergyResults) -> EnergyResults:
    return _aggregate_per_ga_and_brp(production, MeteringPointType.PRODUCTION)


def aggregate_non_profiled_consumption_ga_brp(
    non_profiled_consumption: EnergyResults,
) -> EnergyResults:
    return _aggregate_per_ga_and_brp(
        non_profiled_consumption,
        MeteringPointType.CONSUMPTION,
    )


def aggregate_flex_consumption_ga_brp(flex_consumption: EnergyResults) -> EnergyResults:
    return _aggregate_per_ga_and_brp(
        flex_consumption,
        MeteringPointType.CONSUMPTION,
    )


# Function to aggregate sum per grid area and balance responsible party
def _aggregate_per_ga_and_brp(
    df: EnergyResults,
    market_evaluation_point_type: MeteringPointType,
) -> EnergyResults:
    group_by = [Colname.grid_area, Colname.balance_responsible_id, Colname.time_window]
    result = t.aggregate_sum_quantity_and_qualities(df.df, group_by)

    result = result.select(
        Colname.grid_area,
        Colname.balance_responsible_id,
        Colname.time_window,
        Colname.qualities,
        Colname.sum_quantity,
        lit(market_evaluation_point_type.value).alias(Colname.metering_point_type),
    )
    return EnergyResults(result)


def aggregate_production_ga(production: EnergyResults) -> EnergyResults:
    return _aggregate_per_ga(
        production,
        MeteringPointType.PRODUCTION,
    )


def aggregate_non_profiled_consumption_ga(consumption: EnergyResults) -> EnergyResults:
    return _aggregate_per_ga(
        consumption,
        MeteringPointType.CONSUMPTION,
    )


def aggregate_flex_consumption_ga(
    flex_consumption: EnergyResults,
) -> EnergyResults:
    return _aggregate_per_ga(
        flex_consumption,
        MeteringPointType.CONSUMPTION,
    )


def _aggregate_per_ga(
    df: EnergyResults,
    market_evaluation_point_type: MeteringPointType,
) -> EnergyResults:
    group_by = [Colname.grid_area, Colname.time_window]
    result = t.aggregate_sum_quantity_and_qualities(df.df, group_by)

    result = result.select(
        Colname.grid_area,
        Colname.time_window,
        Colname.qualities,
        Colname.sum_quantity,
        lit(market_evaluation_point_type.value).alias(Colname.metering_point_type),
    )

    return EnergyResults(result, True)

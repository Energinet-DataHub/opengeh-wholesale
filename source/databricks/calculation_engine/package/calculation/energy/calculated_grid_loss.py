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

from package.calculation.energy.data_structures.energy_results import EnergyResults
from package.calculation.preparation.data_structures.prepared_metering_point_time_series import (
    PreparedMeteringPointTimeSeries,
)
from package.codelists import (
    MeteringPointType,
    SettlementMethod,
    MeteringPointResolution,
    QuantityQuality,
)
from package.constants import Colname


def add_calculated_grid_loss_to_metering_point_times_series(
    prepared_metering_point_time_series: PreparedMeteringPointTimeSeries,
    positive_grid_loss: EnergyResults,
    negative_grid_loss: EnergyResults,
) -> PreparedMeteringPointTimeSeries:
    """
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
            f.col(Colname.grid_area),
            f.col(Colname.to_grid_area),
            f.col(Colname.from_grid_area),
            f.col(Colname.metering_point_id),
            f.col(Colname.metering_point_type),
            # TODO: fix
            f.lit(MeteringPointResolution.QUARTER.value).alias(
                Colname.resolution
            ),  # This will change when we must support HOURLY for calculations before 1st of May 2023
            f.col(Colname.observation_time),
            f.col(Colname.quantity).alias(Colname.quantity),
            f.lit(QuantityQuality.CALCULATED.value).alias(Colname.quality),
            f.col(Colname.energy_supplier_id),
            f.col(Colname.balance_responsible_id),
            f.col(Colname.settlement_method),
        )
        .union(prepared_metering_point_time_series.df)
    )

    return PreparedMeteringPointTimeSeries(df)

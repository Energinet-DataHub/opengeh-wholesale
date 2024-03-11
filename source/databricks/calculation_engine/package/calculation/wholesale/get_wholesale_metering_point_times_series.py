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
from pyspark.sql import DataFrame

from package.calculation.energy.energy_results import EnergyResults
from package.codelists import SettlementMethod
from package.constants import Colname


def get_wholesale_metering_point_times_series(
    metering_point_time_series: DataFrame,
    positive_grid_loss: EnergyResults,
    negative_grid_loss: EnergyResults,
) -> DataFrame:
    """
    Metering point time series for wholesale calculation includes all calculation input metering point time series,
    and positive and negative grid loss metering point time series.
    """

    # Union positive and negative grid loss metering point time series and transform them to the same format as the
    # calculation input metering point time series before final union.
    return (
        positive_grid_loss.df.union(negative_grid_loss.df)
        .select(
            f.col(Colname.grid_area),
            f.col(Colname.to_grid_area),
            f.col(Colname.from_grid_area),
            f.col(Colname.metering_point_id),
            f.col(Colname.metering_point_type),
            f.col(Colname.resolution),
            f.col(Colname.time_window_start).alias(Colname.observation_time),
            f.col(Colname.sum_quantity).alias(Colname.quantity),
            # Quality for grid loss is always "calculated"
            f.col(Colname.qualities)[0].alias(Colname.quality),
            f.col(Colname.energy_supplier_id),
            f.col(Colname.balance_responsible_id),
            # It would make sense to get the settlement method from the input data, but it is not available
            # in EnergyResults and it didn't seem worth the effort at the moment of this implementation.
            f.lit(SettlementMethod.FLEX.value).alias(Colname.settlement_method),
        )
        .union(metering_point_time_series)
    )

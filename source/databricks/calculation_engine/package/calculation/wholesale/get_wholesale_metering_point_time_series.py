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

from package.codelists import (
    SettlementMethod,
    MeteringPointType,
    MeteringPointResolution,
)
from package.constants import EnergyResultColumnNames, Colname


def get_wholesale_metering_point_times_series(
    metering_point_time_series: DataFrame,
    positive_grid_loss: DataFrame,
    negative_grid_loss: DataFrame,
) -> DataFrame:
    """
    Metering point time series for wholesale calculation includes all calculation input metering point time series,
    and positive and negative grid loss metering point time series.
    """
    positive_grid_loss_transformed = _transform(
        positive_grid_loss, MeteringPointType.CONSUMPTION
    )
    negative_grid_loss_transformed = _transform(
        negative_grid_loss, MeteringPointType.PRODUCTION
    )

    return metering_point_time_series.union(positive_grid_loss_transformed).union(
        negative_grid_loss_transformed
    )


def _transform(
    grid_loss: DataFrame, metering_point_type: MeteringPointType
) -> DataFrame:
    """
    Transforms calculated grid loss dataframes to the format of the calculation input metering point time series.
    """
    return grid_loss.select(
        f.col(EnergyResultColumnNames.grid_area).alias(Colname.grid_area),
        f.lit(None).alias(Colname.to_grid_area),
        f.lit(None).alias(Colname.from_grid_area),
        f.col(EnergyResultColumnNames.metering_point_id).alias(
            Colname.metering_point_id
        ),
        f.lit(metering_point_type.value).alias(Colname.metering_point_type),
        f.lit(MeteringPointResolution.QUARTER.value).alias(
            Colname.resolution
        ),  # This will change when we must support HOURLY before 1st of May 2023
        f.col(EnergyResultColumnNames.time).alias(Colname.observation_time),
        f.col(EnergyResultColumnNames.quantity).alias(Colname.quantity),
        # Quality for grid loss is always "calculated"
        f.col(EnergyResultColumnNames.quantity_qualities)[0].alias(Colname.quality),
        f.col(EnergyResultColumnNames.energy_supplier_id).alias(
            Colname.energy_supplier_id
        ),
        f.col(EnergyResultColumnNames.balance_responsible_id).alias(
            Colname.balance_responsible_id
        ),
        f.lit(SettlementMethod.FLEX.value).alias(Colname.settlement_method),
    )

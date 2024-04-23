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

from package.calculation.preparation.data_structures.metering_point_time_series import (
    MeteringPointTimeSeries,
)
from package.constants import Colname, MeteringPointPeriodColname, TimeSeriesColname
from package.infrastructure import logging_configuration


@logging_configuration.use_span("get_metering_point_periods_basis_data")
def get_metering_point_periods_basis_data(
    calculation_id: str,
    metering_point_df: DataFrame,
) -> DataFrame:
    return metering_point_df.select(
        f.lit(calculation_id).alias(MeteringPointPeriodColname.calculation_id),
        f.col(Colname.metering_point_id).alias(
            MeteringPointPeriodColname.metering_point_id
        ),
        f.col(Colname.metering_point_type).alias(
            MeteringPointPeriodColname.metering_point_type
        ),
        f.col(Colname.settlement_method).alias(
            MeteringPointPeriodColname.settlement_method
        ),
        f.col(Colname.grid_area).alias(MeteringPointPeriodColname.grid_area),
        f.col(Colname.resolution).alias(MeteringPointPeriodColname.resolution),
        f.col(Colname.from_grid_area).alias(MeteringPointPeriodColname.from_grid_area),
        f.col(Colname.to_grid_area).alias(MeteringPointPeriodColname.to_grid_area),
        f.col(Colname.parent_metering_point_id).alias(
            MeteringPointPeriodColname.parent_metering_point_id
        ),
        f.col(Colname.energy_supplier_id).alias(
            MeteringPointPeriodColname.energy_supplier_id
        ),
        f.col(Colname.balance_responsible_id).alias(
            MeteringPointPeriodColname.balance_responsible_id
        ),
        f.col(Colname.from_date).alias(MeteringPointPeriodColname.from_date),
        f.col(Colname.to_date).alias(MeteringPointPeriodColname.to_date),
    )


@logging_configuration.use_span("get_time_series_points_basis_data")
def get_time_series_points_basis_data(
    calculation_id: str,
    metering_point_time_series: MeteringPointTimeSeries,
) -> DataFrame:
    return metering_point_time_series.df.select(
        f.lit(calculation_id).alias(TimeSeriesColname.calculation_id),
        f.col(Colname.metering_point_id).alias(TimeSeriesColname.metering_point_id),
        f.col(Colname.quantity).alias(TimeSeriesColname.quantity),
        f.col(Colname.quality).alias(TimeSeriesColname.quality),
        f.col(Colname.observation_time).alias(TimeSeriesColname.observation_time),
    )

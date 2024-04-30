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
import pyspark.sql.functions as F
from pyspark.sql.types import DecimalType
from pyspark.sql import DataFrame

from package.calculation.preparation.data_structures.prepared_metering_point_time_series import (
    PreparedMeteringPointTimeSeries,
)
from package.constants import (
    Colname,
    MeteringPointPeriodColname,
    TimeSeriesColname,
    ChargeMasterDataPeriodsColname,
    ChargePricePointsColname,
    ChargeLinkPeriodsColname,
)
from package.infrastructure import logging_configuration

from package.calculation.preparation.data_structures import InputChargesContainer


@logging_configuration.use_span("get_metering_point_periods_basis_data")
def get_metering_point_periods_basis_data(
    calculation_id: str,
    metering_point_df: DataFrame,
) -> DataFrame:
    return metering_point_df.select(
        F.lit(calculation_id).alias(MeteringPointPeriodColname.calculation_id),
        F.col(Colname.metering_point_id).alias(
            MeteringPointPeriodColname.metering_point_id
        ),
        F.col(Colname.metering_point_type).alias(
            MeteringPointPeriodColname.metering_point_type
        ),
        F.col(Colname.settlement_method).alias(
            MeteringPointPeriodColname.settlement_method
        ),
        F.col(Colname.grid_area).alias(MeteringPointPeriodColname.grid_area),
        F.col(Colname.resolution).alias(MeteringPointPeriodColname.resolution),
        F.col(Colname.from_grid_area).alias(MeteringPointPeriodColname.from_grid_area),
        F.col(Colname.to_grid_area).alias(MeteringPointPeriodColname.to_grid_area),
        F.col(Colname.parent_metering_point_id).alias(
            MeteringPointPeriodColname.parent_metering_point_id
        ),
        F.col(Colname.energy_supplier_id).alias(
            MeteringPointPeriodColname.energy_supplier_id
        ),
        F.col(Colname.balance_responsible_id).alias(
            MeteringPointPeriodColname.balance_responsible_id
        ),
        F.col(Colname.from_date).alias(MeteringPointPeriodColname.from_date),
        F.col(Colname.to_date).alias(MeteringPointPeriodColname.to_date),
    )


@logging_configuration.use_span("get_time_series_points_basis_data")
def get_time_series_points_basis_data(
    calculation_id: str,
    metering_point_time_series: PreparedMeteringPointTimeSeries,
) -> DataFrame:
    return metering_point_time_series.df.select(
        F.lit(calculation_id).alias(TimeSeriesColname.calculation_id),
        F.col(Colname.metering_point_id).alias(TimeSeriesColname.metering_point_id),
        F.col(Colname.quantity)
        .alias(TimeSeriesColname.quantity)
        .cast(DecimalType(18, 3)),
        F.col(Colname.quality).alias(TimeSeriesColname.quality),
        F.col(Colname.observation_time).alias(TimeSeriesColname.observation_time),
    )


@logging_configuration.use_span("get_charge_master_data_basis_data")
def get_charge_master_data_basis_data(
    calculation_id: str,
    input_charges_container: InputChargesContainer,
) -> DataFrame:
    if input_charges_container:
        return input_charges_container.charge_master_data._df.select(
            F.lit(calculation_id).alias(ChargeMasterDataPeriodsColname.calculation_id),
            F.col(Colname.charge_key).alias(ChargeMasterDataPeriodsColname.charge_key),
            F.col(Colname.charge_code).alias(
                ChargeMasterDataPeriodsColname.charge_code
            ),
            F.col(Colname.charge_type).alias(
                ChargeMasterDataPeriodsColname.charge_type
            ),
            F.col(Colname.charge_owner).alias(ChargeMasterDataPeriodsColname.charge_owner_id),
            F.col(Colname.resolution).alias(ChargeMasterDataPeriodsColname.resolution),
            F.col(Colname.charge_tax).alias(ChargeMasterDataPeriodsColname.is_tax),
            F.col(Colname.from_date).alias(ChargeMasterDataPeriodsColname.from_date),
            F.col(Colname.to_date).alias(ChargeMasterDataPeriodsColname.to_date),
        )
    else:
        return None


@logging_configuration.use_span("get_charge_prices_basis_data")
def get_charge_prices_basis_data(
    calculation_id: str,
    input_charges_container: InputChargesContainer,
) -> DataFrame:
    if input_charges_container:
        return input_charges_container.charge_prices._df.select(
            F.lit(calculation_id).alias(ChargePricePointsColname.calculation_id),
            F.col(Colname.charge_key).alias(ChargePricePointsColname.charge_key),
            F.col(Colname.charge_code).alias(ChargePricePointsColname.charge_code),
            F.col(Colname.charge_type).alias(ChargePricePointsColname.charge_type),
            F.col(Colname.charge_owner).alias(ChargePricePointsColname.charge_owner_id),
            F.col(Colname.charge_price).alias(ChargePricePointsColname.charge_price),
            F.col(Colname.charge_time).alias(ChargePricePointsColname.charge_time),
        )
    else:
        return None


@logging_configuration.use_span("get_charge_links_basis_data")
def get_charge_links_basis_data(
    calculation_id: str,
    input_charges_container: InputChargesContainer,
) -> DataFrame:
    if input_charges_container:
        return input_charges_container.charge_links.select(
            F.lit(calculation_id).alias(ChargeLinkPeriodsColname.calculation_id),
            F.col(Colname.charge_key).alias(ChargeLinkPeriodsColname.charge_key),
            F.col(Colname.charge_code).alias(ChargeLinkPeriodsColname.charge_code),
            F.col(Colname.charge_type).alias(ChargeLinkPeriodsColname.charge_type),
            F.col(Colname.charge_owner).alias(ChargeLinkPeriodsColname.charge_owner_id),
            F.col(Colname.metering_point_id).alias(
                ChargeLinkPeriodsColname.metering_point_id
            ),
            F.col(Colname.quantity).alias(ChargeLinkPeriodsColname.quantity),
            F.col(Colname.from_date).alias(ChargeLinkPeriodsColname.from_date),
            F.col(Colname.to_date).alias(ChargeLinkPeriodsColname.to_date),
        )
    else:
        return None

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

from pyspark.sql import DataFrame

from package.calculation.calculation_output import BasisDataOutput
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.preparation.data_structures import InputChargesContainer
from package.calculation.preparation.data_structures.grid_loss_metering_point_ids import (
    GridLossMeteringPointIds,
)
from package.calculation.preparation.data_structures.prepared_metering_point_time_series import (
    PreparedMeteringPointTimeSeries,
)
from package.databases.wholesale_basis_data_internal import basis_data
from package.infrastructure import logging_configuration


@logging_configuration.use_span("calculation.basis_data.prepare")
def create(
    args: CalculatorArgs,
    metering_point_periods_df: DataFrame,
    metering_point_time_series_df: PreparedMeteringPointTimeSeries,
    input_charges_container: InputChargesContainer | None,
    grid_loss_metering_point_ids: GridLossMeteringPointIds,
) -> BasisDataOutput:
    time_series_points_basis_data = basis_data.get_time_series_points_basis_data(
        args.calculation_id, metering_point_time_series_df
    )

    metering_point_periods_basis_data = (
        basis_data.get_metering_point_periods_basis_data(
            args.calculation_id, metering_point_periods_df
        )
    )

    grid_loss_metering_point_ids_basis_data = (
        basis_data.get_grid_loss_metering_point_ids_basis_data(
            args.calculation_id, grid_loss_metering_point_ids
        )
    )

    if input_charges_container:
        charge_price_information_basis_data = (
            basis_data.get_charge_price_information_basis_data(
                args.calculation_id, input_charges_container
            )
        )

        charge_prices_basis_data = basis_data.get_charge_prices_basis_data(
            args.calculation_id, input_charges_container
        )

        charge_links_basis_data = basis_data.get_charge_links_basis_data(
            args.calculation_id, input_charges_container
        )
    else:
        charge_price_information_basis_data = None
        charge_prices_basis_data = None
        charge_links_basis_data = None

    return BasisDataOutput(
        time_series_points=time_series_points_basis_data,
        metering_point_periods=metering_point_periods_basis_data,
        charge_price_information_periods=charge_price_information_basis_data,
        charge_price_points=charge_prices_basis_data,
        charge_link_periods=charge_links_basis_data,
        grid_loss_metering_point_ids=grid_loss_metering_point_ids_basis_data,
    )

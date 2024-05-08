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

from package.calculation.calculation_results import BasisDataContainer
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.preparation.data_structures.prepared_metering_point_time_series import (
    PreparedMeteringPointTimeSeries,
)
from package.calculation.basis_data import basis_data
from package.infrastructure import logging_configuration
from package.calculation.preparation.data_structures import InputChargesContainer


@logging_configuration.use_span("calculation.basis_data.prepare")
def create(
    args: CalculatorArgs,
    metering_point_periods_df: DataFrame,
    metering_point_time_series_df: PreparedMeteringPointTimeSeries,
    input_charges_container: InputChargesContainer | None,
) -> BasisDataContainer:
    time_series_points_basis_data = basis_data.get_time_series_points_basis_data(
        args.calculation_id, metering_point_time_series_df
    )

    metering_point_periods_basis_data = (
        basis_data.get_metering_point_periods_basis_data(
            args.calculation_id, metering_point_periods_df
        )
    )

    if input_charges_container:
        charge_master_data_basis_data = basis_data.get_charge_master_data_basis_data(
            args.calculation_id, input_charges_container
        )

        charge_prices_basis_data = basis_data.get_charge_prices_basis_data(
            args.calculation_id, input_charges_container
        )

        charge_links_basis_data = basis_data.get_charge_links_basis_data(
            args.calculation_id, input_charges_container
        )
    else:
        charge_master_data_basis_data = None
        charge_prices_basis_data = None
        charge_links_basis_data = None

    return BasisDataContainer(
        time_series_points=time_series_points_basis_data,
        metering_point_periods=metering_point_periods_basis_data,
        charge_master_data=charge_master_data_basis_data,
        charge_prices=charge_prices_basis_data,
        charge_links=charge_links_basis_data,
    )

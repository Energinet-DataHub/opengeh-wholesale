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

from package.calculation.energy.calculated_grid_loss import (
    add_calculated_grid_loss_to_metering_point_times_series,
)
from package.calculation.preparation.transformations.grid_loss_metering_points import (
    get_grid_loss_metering_points,
)
from package.calculation.preparation.transformations.metering_point_periods_for_calculation_type import (
    get_metering_points_periods_for_wholesale_basis_data,
    is_parent_metering_point,
    get_metering_point_periods_for_wholesale_calculation,
)
from package.databases.wholesale_basis_data_internal import basis_data_factory

from package.infrastructure import logging_configuration
from .calculation_output import (
    CalculationOutput,
)
from .calculator_args import CalculatorArgs
from .energy import energy_calculation
from .preparation import PreparedDataReader
from .preparation.data_structures import PreparedMeteringPointTimeSeries
from .wholesale import wholesale_calculation
from ..codelists.calculation_type import is_wholesale_calculation_type
from ..constants import Colname


class CalculationCore:

    @staticmethod
    def execute(
        args: CalculatorArgs,
        prepared_data_reader: PreparedDataReader,
    ) -> CalculationOutput:
        if is_wholesale_calculation_type(args.calculation_type):
            return CalculationCore._execute_wholesale(args, prepared_data_reader)
        else:
            return CalculationCore._execute_energy(args, prepared_data_reader)

    @staticmethod
    @logging_configuration.use_span("calculation.wholesale.prepare")
    def _execute_wholesale(
        args: CalculatorArgs,
        prepared_data_reader: PreparedDataReader,
    ) -> CalculationOutput:
        calculation_output = CalculationOutput()

        # cache of metering_point_time_series had no effect on performance (01-12-2023)
        all_metering_point_periods = prepared_data_reader.get_metering_point_periods_df(
            args.calculation_period_start_datetime,
            args.calculation_period_end_datetime,
            args.calculation_grid_areas,
        )

        all_metering_point_periods = (
            get_metering_points_periods_for_wholesale_basis_data(
                all_metering_point_periods
            )
        )

        grid_loss_responsible_df = prepared_data_reader.get_grid_loss_responsible(
            args.calculation_grid_areas, all_metering_point_periods
        )

        metering_point_periods_df_without_grid_loss = (
            prepared_data_reader.get_metering_point_periods_without_grid_loss(
                all_metering_point_periods
            )
        )

        grid_loss_metering_points_df = get_grid_loss_metering_points(
            grid_loss_responsible_df
        )

        metering_point_time_series = (
            prepared_data_reader.get_metering_point_time_series(
                args.calculation_period_start_datetime,
                args.calculation_period_end_datetime,
                metering_point_periods_df_without_grid_loss,
            )
        )
        metering_point_time_series.cache_internal()

        (
            calculation_output.energy_results_output,
            positive_grid_loss,
            negative_grid_loss,
        ) = energy_calculation.execute(
            args,
            metering_point_time_series,
            grid_loss_responsible_df,
        )

        # This extends the content of metering_point_time_series with calculated grid loss,
        # which is used in the wholesale calculation and the basis data
        metering_point_time_series = (
            add_calculated_grid_loss_to_metering_point_times_series(
                args,
                metering_point_time_series,
                positive_grid_loss,
                negative_grid_loss,
            )
        )

        # Extract metering point ids from all metering point periods in
        # the grid areas specified in the calculation arguments.
        metering_point_period_ids = all_metering_point_periods.select(
            Colname.metering_point_id
        ).distinct()

        input_charges = prepared_data_reader.get_input_charges(
            args.calculation_period_start_datetime,
            args.calculation_period_end_datetime,
            metering_point_period_ids,
        )

        metering_point_periods_for_basis_data = all_metering_point_periods

        metering_point_periods_for_wholesale_calculation = (
            get_metering_point_periods_for_wholesale_calculation(
                metering_point_periods_for_basis_data
            )
        )

        prepared_charges = prepared_data_reader.get_prepared_charges(
            metering_point_periods_for_wholesale_calculation,
            metering_point_time_series,
            input_charges,
            args.time_zone,
        )

        calculation_output.wholesale_results_output = wholesale_calculation.execute(
            args,
            prepared_charges,
        )

        # Add basis data to results
        calculation_output.basis_data_output = basis_data_factory.create(
            args,
            metering_point_periods_for_basis_data,
            metering_point_time_series,
            input_charges,
            grid_loss_metering_points_df,
        )

        return calculation_output

    @staticmethod
    @logging_configuration.use_span("calculation.energy.prepare")
    def _execute_energy(
        args: CalculatorArgs,
        prepared_data_reader: PreparedDataReader,
    ) -> CalculationOutput:
        calculation_output = CalculationOutput()

        # cache of metering_point_time_series had no effect on performance (01-12-2023)
        all_metering_point_periods = prepared_data_reader.get_metering_point_periods_df(
            args.calculation_period_start_datetime,
            args.calculation_period_end_datetime,
            args.calculation_grid_areas,
        )

        grid_loss_responsible_df = prepared_data_reader.get_grid_loss_responsible(
            args.calculation_grid_areas, all_metering_point_periods
        )

        metering_point_periods_df_without_grid_loss = (
            prepared_data_reader.get_metering_point_periods_without_grid_loss(
                all_metering_point_periods
            )
        )

        grid_loss_metering_points_df = get_grid_loss_metering_points(
            grid_loss_responsible_df
        )

        metering_point_time_series = (
            prepared_data_reader.get_metering_point_time_series(
                args.calculation_period_start_datetime,
                args.calculation_period_end_datetime,
                metering_point_periods_df_without_grid_loss,
            )
        )
        metering_point_time_series.cache_internal()

        (
            calculation_output.energy_results_output,
            positive_grid_loss,
            negative_grid_loss,
        ) = energy_calculation.execute(
            args,
            metering_point_time_series,
            grid_loss_responsible_df,
        )

        # This extends the content of metering_point_time_series with calculated grid loss,
        # which is used in the wholesale calculation and the basis data
        metering_point_time_series = (
            add_calculated_grid_loss_to_metering_point_times_series(
                args,
                metering_point_time_series,
                positive_grid_loss,
                negative_grid_loss,
            )
        )

        metering_point_periods_for_basis_data = all_metering_point_periods.where(
            is_parent_metering_point(Colname.metering_point_type)
        )
        metering_point_time_series = PreparedMeteringPointTimeSeries(
            metering_point_time_series.df.where(
                is_parent_metering_point(Colname.metering_point_type)
            )
        )

        # Add basis data to results
        calculation_output.basis_data_output = basis_data_factory.create(
            args,
            metering_point_periods_for_basis_data,
            metering_point_time_series,
            input_charges=None,
            grid_loss_metering_points_df=grid_loss_metering_points_df,
        )

        return calculation_output

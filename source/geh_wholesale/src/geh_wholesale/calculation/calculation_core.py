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
from geh_common.telemetry import use_span
from pyspark.sql import DataFrame

from geh_wholesale.calculation.energy.calculated_grid_loss import (
    append_calculated_grid_loss_to_metering_point_times_series,
)
from geh_wholesale.calculation.preparation.transformations.metering_point_periods_for_calculation_type import (
    add_parent_data_to_child_metering_point_periods,
    is_parent_metering_point,
)
from geh_wholesale.databases.wholesale_basis_data_internal import basis_data_factory

from ..codelists import MeteringPointType
from ..codelists.calculation_type import is_wholesale_calculation_type
from ..constants import Colname
from .calculation_output import (
    CalculationOutput,
)
from .calculator_args import CalculatorArgs
from .energy import energy_calculation
from .preparation import PreparedDataReader
from .preparation.transformations import get_grid_loss_metering_point_ids
from .wholesale import wholesale_calculation


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
    @use_span("calculation.wholesale.prepare")
    def _execute_wholesale(
        args: CalculatorArgs,
        prepared_data_reader: PreparedDataReader,
    ) -> CalculationOutput:
        calculation_output = CalculationOutput()

        metering_point_periods = _get_metering_point_periods(args, prepared_data_reader)

        metering_point_periods__except_grid_loss = prepared_data_reader.get_metering_point_periods__except_grid_loss(
            metering_point_periods
        )

        metering_point_time_series = prepared_data_reader.get_metering_point_time_series(
            args.period_start_datetime,
            args.period_end_datetime,
            metering_point_periods__except_grid_loss,
        )
        metering_point_time_series.cache_internal()

        grid_loss_metering_point_periods = prepared_data_reader.get_grid_loss_metering_point_periods(
            args.grid_areas, metering_point_periods
        )

        (
            calculation_output.energy_results_output,
            positive_grid_loss,
            negative_grid_loss,
        ) = energy_calculation.execute(
            args,
            metering_point_time_series,
            grid_loss_metering_point_periods,
        )

        # This extends the content of metering_point_time_series with calculated grid loss,
        # which is used in the wholesale calculation and the basis data
        metering_point_time_series = append_calculated_grid_loss_to_metering_point_times_series(
            args,
            metering_point_time_series,
            positive_grid_loss,
            negative_grid_loss,
        )

        # Extract metering point ids from all metering point periods in
        # the grid areas specified in the calculation arguments.
        metering_point_period_ids = metering_point_periods.select(Colname.metering_point_id).distinct()

        input_charges = prepared_data_reader.get_input_charges(
            args.period_start_datetime,
            args.period_end_datetime,
            metering_point_period_ids,
        )

        metering_point_periods_for_wholesale_calculation = metering_point_periods.where(
            f.col(Colname.metering_point_type) != MeteringPointType.EXCHANGE.value
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

        grid_loss_metering_point_ids = get_grid_loss_metering_point_ids(grid_loss_metering_point_periods)

        # Add basis data to results
        calculation_output.basis_data_output = basis_data_factory.create(
            args,
            metering_point_periods,
            metering_point_time_series,
            grid_loss_metering_point_ids,
            input_charges,
        )

        return calculation_output

    @staticmethod
    @use_span("calculation.energy.prepare")
    def _execute_energy(
        args: CalculatorArgs,
        prepared_data_reader: PreparedDataReader,
    ) -> CalculationOutput:
        calculation_output = CalculationOutput()

        # cache of metering point time series had no effect on performance (01-12-2023)
        parent_metering_point_periods = prepared_data_reader.get_metering_point_periods(
            args.period_start_datetime,
            args.period_end_datetime,
            args.grid_areas,
        ).where(is_parent_metering_point(Colname.metering_point_type))

        grid_loss_metering_point_periods = prepared_data_reader.get_grid_loss_metering_point_periods(
            args.grid_areas, parent_metering_point_periods
        )

        metering_point_periods__except_grid_loss = prepared_data_reader.get_metering_point_periods__except_grid_loss(
            parent_metering_point_periods
        )

        parent_metering_point_time_series__except_grid_loss = prepared_data_reader.get_metering_point_time_series(
            args.period_start_datetime,
            args.period_end_datetime,
            metering_point_periods__except_grid_loss,
        )
        parent_metering_point_time_series__except_grid_loss.cache_internal()

        (
            calculation_output.energy_results_output,
            positive_grid_loss,
            negative_grid_loss,
        ) = energy_calculation.execute(
            args,
            parent_metering_point_time_series__except_grid_loss,
            grid_loss_metering_point_periods,
        )

        # This extends the content of metering_point_time_series with calculated grid loss,
        # which is used in the wholesale calculation and the basis data
        parent_metering_point_time_series__including_grid_loss = (
            append_calculated_grid_loss_to_metering_point_times_series(
                args,
                parent_metering_point_time_series__except_grid_loss,
                positive_grid_loss,
                negative_grid_loss,
            )
        )

        grid_loss_metering_point_ids = get_grid_loss_metering_point_ids(grid_loss_metering_point_periods)

        # Add basis data to results

        calculation_output.basis_data_output = basis_data_factory.create(
            args,
            parent_metering_point_periods,
            parent_metering_point_time_series__including_grid_loss,
            grid_loss_metering_point_ids=grid_loss_metering_point_ids,
        )

        return calculation_output


def _get_metering_point_periods(args: CalculatorArgs, prepared_data_reader: PreparedDataReader) -> DataFrame:
    # cache of metering_point_time_series had no effect on performance (01-12-2023)
    metering_point_periods = prepared_data_reader.get_metering_point_periods(
        args.period_start_datetime,
        args.period_end_datetime,
        args.grid_areas,
    )
    # Child metering points inherit energy supplier and balance responsible party from
    # their parent metering point. So we add this data to the child metering points.
    metering_point_periods = add_parent_data_to_child_metering_point_periods(metering_point_periods)
    return metering_point_periods

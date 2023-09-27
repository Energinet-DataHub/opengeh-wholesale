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


from package.codelists import ProcessType
from package.calculation_input import CalculationInput
from package.calculation_output.wholesale_calculation_result_writer import (
    WholesaleCalculationResultWriter,
)
from .calculator_args import CalculatorArgs
from .energy import energy_calculation
from .wholesale import wholesale_calculation
from . import setup


def execute(args: CalculatorArgs, calculation_input: CalculationInput) -> None:
    metering_point_periods_df = calculation_input.get_metering_point_periods_df(
        args.batch_period_start_datetime,
        args.batch_period_end_datetime,
        args.batch_grid_areas,
    )
    time_series_points_df = calculation_input.get_time_series_points()
    grid_loss_responsible_df = calculation_input.get_grid_loss_responsible(
        args.batch_grid_areas
    )

    enriched_time_series_point_df = setup.get_enriched_time_series_points_df(
        time_series_points_df,
        metering_point_periods_df,
        args.batch_period_start_datetime,
        args.batch_period_end_datetime,
    )

    energy_calculation.execute(
        args.batch_id,
        args.batch_process_type,
        args.batch_execution_time_start,
        args.wholesale_container_path,
        metering_point_periods_df,
        enriched_time_series_point_df,
        grid_loss_responsible_df,
        args.time_zone,
    )

    if (
        args.batch_process_type == ProcessType.WHOLESALE_FIXING
        or args.batch_process_type == ProcessType.FIRST_CORRECTION_SETTLEMENT
        or args.batch_process_type == ProcessType.SECOND_CORRECTION_SETTLEMENT
        or args.batch_process_type == ProcessType.THIRD_CORRECTION_SETTLEMENT
    ):
        wholesale_calculation_result_writer = WholesaleCalculationResultWriter(
            args.batch_id, args.batch_process_type, args.batch_execution_time_start
        )

        charges_df = calculation_input.get_charges()

        wholesale_calculation.execute(
            wholesale_calculation_result_writer,
            metering_point_periods_df,
            time_series_points_df,
            charges_df,
            args.batch_period_start_datetime,
        )

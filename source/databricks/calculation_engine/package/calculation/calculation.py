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
import pyspark.sql.functions as F

from package.codelists import ChargeResolution, MeteringPointType, ProcessType
from package.calculation_output.wholesale_calculation_result_writer import (
    WholesaleCalculationResultWriter,
)
from package.constants import Colname
from package.calculation_output.basis_data_writer import BasisDataWriter

from .preparation import PreparedDataReader
from .calculator_args import CalculatorArgs
from .energy import energy_calculation
from .wholesale import wholesale_calculation
from ..common.logger import Logger

logger = Logger(__name__)


def execute(args: CalculatorArgs, prepared_data_reader: PreparedDataReader) -> None:
    logger.info("Starting calculation")

    # cache of metering_point_time_series had no effect on performance (01-12-2023)
    metering_point_periods_df = prepared_data_reader.get_metering_point_periods_df(
        args.batch_period_start_datetime,
        args.batch_period_end_datetime,
        args.batch_grid_areas,
    )
    grid_loss_responsible_df = prepared_data_reader.get_grid_loss_responsible(
        args.batch_grid_areas
    )

    metering_point_time_series = prepared_data_reader.get_metering_point_time_series(
        args.batch_period_start_datetime,
        args.batch_period_end_datetime,
        metering_point_periods_df,
    ).cache()

    if args.basis_data_write_only:
        logger.info("Starting basis data writer")
        basis_data_writer = BasisDataWriter(args.wholesale_container_path, args.batch_id)
        basis_data_writer.write(
            metering_point_periods_df,
            metering_point_time_series,
            args.time_zone,
        )

    else:
        logger.info("Starting energy calculation")
        energy_calculation.execute(
            args.batch_id,
            args.batch_process_type,
            args.batch_execution_time_start,
            args.batch_grid_areas,
            metering_point_time_series,
            grid_loss_responsible_df,
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

            charges_df = prepared_data_reader.get_charges()
            metering_points_periods_df = _get_production_and_consumption_metering_points(
                metering_point_periods_df
            )

            tariffs_hourly_df = prepared_data_reader.get_tariff_charges(
                metering_points_periods_df,
                metering_point_time_series,
                charges_df,
                ChargeResolution.HOUR,
            )

            wholesale_calculation.execute(
                wholesale_calculation_result_writer,
                tariffs_hourly_df,
                args.batch_period_start_datetime,
            )


def _get_production_and_consumption_metering_points(
    metering_points_periods_df: DataFrame,
) -> DataFrame:
    return metering_points_periods_df.filter(
        (F.col(Colname.metering_point_type) == MeteringPointType.CONSUMPTION.value)
        | (F.col(Colname.metering_point_type) == MeteringPointType.PRODUCTION.value)
    )

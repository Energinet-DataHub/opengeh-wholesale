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
    ChargeResolution,
    MeteringPointType,
    CalculationType,
)
from package.constants import Colname
from package.infrastructure import logging_configuration
from .CalculationResults import (
    CalculationResultsContainer,
)
from .calculator_args import CalculatorArgs
from .energy import energy_calculation
from .output import basis_data_factory
from .output.basis_data_results import write_basis_data
from .output.energy_results import write_energy_results
from .output.wholesale_results import write as write_wholesale_results
from .preparation import PreparedDataReader
from .wholesale import wholesale_calculation


def execute(args: CalculatorArgs, prepared_data_reader: PreparedDataReader) -> None:
    results = _execute(args, prepared_data_reader)
    _write_results(args, results)


@logging_configuration.use_span("calculation")
def _execute(
    args: CalculatorArgs, prepared_data_reader: PreparedDataReader
) -> CalculationResultsContainer:
    results = CalculationResultsContainer()

    with logging_configuration.start_span("calculation.prepare"):
        # cache of metering_point_time_series had no effect on performance (01-12-2023)
        metering_point_periods_df = prepared_data_reader.get_metering_point_periods_df(
            args.calculation_period_start_datetime,
            args.calculation_period_end_datetime,
            args.calculation_grid_areas,
        )
        grid_loss_responsible_df = prepared_data_reader.get_grid_loss_responsible(
            args.calculation_grid_areas, metering_point_periods_df
        )

        metering_point_time_series = (
            prepared_data_reader.get_metering_point_time_series(
                args.calculation_period_start_datetime,
                args.calculation_period_end_datetime,
                metering_point_periods_df,
            ).cache()
        )

    results.energy_results = energy_calculation.execute(
        args.calculation_type,
        args.calculation_grid_areas,
        metering_point_time_series,
        grid_loss_responsible_df,
    )

    if (
        args.calculation_type == CalculationType.WHOLESALE_FIXING
        or args.calculation_type == CalculationType.FIRST_CORRECTION_SETTLEMENT
        or args.calculation_type == CalculationType.SECOND_CORRECTION_SETTLEMENT
        or args.calculation_type == CalculationType.THIRD_CORRECTION_SETTLEMENT
    ):
        charge_master_data = prepared_data_reader.get_charge_master_data(
            args.calculation_period_start_datetime, args.calculation_period_end_datetime
        )

        charge_prices = prepared_data_reader.get_charge_prices(
            args.calculation_period_start_datetime, args.calculation_period_end_datetime
        )

        metering_points_periods_for_wholesale_calculation = (
            _add_energy_supplier_id_to_child_metering_points(metering_point_periods_df)
        )

        charges_link_metering_point_periods = (
            prepared_data_reader.get_charge_link_metering_point_periods(
                args.calculation_period_start_datetime,
                args.calculation_period_end_datetime,
                metering_points_periods_for_wholesale_calculation,
            )
        )

        tariffs_hourly_df = prepared_data_reader.get_tariff_charges(
            metering_point_time_series,
            charge_master_data,
            charge_prices,
            charges_link_metering_point_periods,
            ChargeResolution.HOUR,
            args.time_zone,
        )

        tariffs_daily_df = prepared_data_reader.get_tariff_charges(
            metering_point_time_series,
            charge_master_data,
            charge_prices,
            charges_link_metering_point_periods,
            ChargeResolution.DAY,
            args.time_zone,
        )

        results.wholesale_results = wholesale_calculation.execute(
            args,
            tariffs_hourly_df,
            tariffs_daily_df,
        )

    # Add basis data to results
    results.basis_data = basis_data_factory.create(
        metering_point_periods_df, metering_point_time_series, args.time_zone
    )

    return results


def _get_production_and_consumption_metering_points(
    metering_points_periods_df: DataFrame,
) -> DataFrame:
    return metering_points_periods_df.filter(
        (f.col(Colname.metering_point_type) == MeteringPointType.CONSUMPTION.value)
        | (f.col(Colname.metering_point_type) == MeteringPointType.PRODUCTION.value)
    ).select(Colname.metering_point_id, Colname.energy_supplier_id)


def _get_all_child_metering_points(
    metering_points_periods_df: DataFrame,
) -> DataFrame:
    return metering_points_periods_df.filter(
        (f.col(Colname.metering_point_type) == MeteringPointType.CONSUMPTION.value)
        | (f.col(Colname.metering_point_type) == MeteringPointType.PRODUCTION.value)
        | (f.col(Colname.metering_point_type) == MeteringPointType.VE_PRODUCTION.value)
        | (f.col(Colname.metering_point_type) == MeteringPointType.NET_PRODUCTION.value)
        | (f.col(Colname.metering_point_type) == MeteringPointType.SUPPLY_TO_GRID.value)
        | (
            f.col(Colname.metering_point_type)
            == MeteringPointType.CONSUMPTION_FROM_GRID.value
        )
        | (
            f.col(Colname.metering_point_type)
            == MeteringPointType.WHOLESALE_SERVICES_INFORMATION.value
        )
        | (f.col(Colname.metering_point_type) == MeteringPointType.OWN_PRODUCTION.value)
        | (f.col(Colname.metering_point_type) == MeteringPointType.NET_FROM_GRID.value)
        | (f.col(Colname.metering_point_type) == MeteringPointType.NET_TO_GRID.value)
        | (
            f.col(Colname.metering_point_type)
            == MeteringPointType.TOTAL_CONSUMPTION.value
        )
        | (
            f.col(Colname.metering_point_type)
            == MeteringPointType.ELECTRICAL_HEATING.value
        )
        | (
            f.col(Colname.metering_point_type)
            == MeteringPointType.NET_CONSUMPTION.value
        )
        | (
            f.col(Colname.metering_point_type)
            == MeteringPointType.EFFECT_SETTLEMENT.value
        )
    )


def _add_energy_supplier_id_to_child_metering_points(
    metering_point_periods_df: DataFrame,
) -> DataFrame:
    metering_point_periods_df.show()
    production_and_consumption_metering_points = (
        _get_production_and_consumption_metering_points(metering_point_periods_df)
    )
    es = "energy_supplier_id_temp"
    mp = "metering_point_id_temp"
    production_and_consumption_metering_points = (
        production_and_consumption_metering_points.withColumnRenamed(
            Colname.energy_supplier_id, es
        ).withColumnRenamed(Colname.metering_point_id, mp)
    )
    production_and_consumption_metering_points.show()
    all_metering_points = _get_all_child_metering_points(metering_point_periods_df)
    all_metering_points.show()
    metering_points_periods_for_wholesale_calculation = all_metering_points.join(
        production_and_consumption_metering_points,
        production_and_consumption_metering_points[mp]
        == all_metering_points[
            Colname.parent_metering_point_id
        ],  # parent_metering_point_id is always null on child metering points
        "left",
    )
    metering_points_periods_for_wholesale_calculation.show()
    metering_points_periods_for_wholesale_calculation.select(
        all_metering_points[Colname.metering_point_id],
        all_metering_points[Colname.metering_point_type],
        all_metering_points[Colname.calculation_type],
        all_metering_points[Colname.settlement_method],
        all_metering_points[Colname.grid_area],
        all_metering_points[Colname.resolution],
        all_metering_points[Colname.from_grid_area],
        all_metering_points[Colname.to_grid_area],
        all_metering_points[Colname.parent_metering_point_id],
        production_and_consumption_metering_points[es].alias(
            Colname.energy_supplier_id
        ),  # energy_supplier_id is always null on child metering points
        all_metering_points[Colname.balance_responsible_id],
        all_metering_points[Colname.from_date],
        all_metering_points[Colname.to_date],
    )
    metering_points_periods_for_wholesale_calculation.show()
    return metering_points_periods_for_wholesale_calculation


def _write_results(args: CalculatorArgs, results: CalculationResultsContainer) -> None:
    write_energy_results(args, results.energy_results)
    if results.wholesale_results is not None:
        write_wholesale_results(results.wholesale_results)
    # We write basis data at the end of the calculation to make it easier to analyze performance of the calculation part
    write_basis_data(args, results.basis_data)

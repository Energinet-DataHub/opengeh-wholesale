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
import pyspark.sql.functions as f

from package.codelists import (
    ChargeResolution,
    CalculationType,
    SettlementMethod,
    MeteringPointType,
    MeteringPointResolution,
)
from package.infrastructure import logging_configuration
from .CalculationResults import (
    CalculationResultsContainer,
)
from .calculator_args import CalculatorArgs
from .energy import energy_calculation
from .output import basis_data_factory
from .output.basis_data_results import write_basis_data
from .output.energy_results import write_energy_results
from .output.wholesale_results import write_wholesale_results
from .preparation import PreparedDataReader

from .wholesale import wholesale_calculation
from .wholesale.get_metering_points_and_child_metering_points import (
    get_metering_points_and_child_metering_points,
)
from ..constants import EnergyResultColumnNames, Colname


@logging_configuration.use_span("calculation")
def execute(args: CalculatorArgs, prepared_data_reader: PreparedDataReader) -> None:
    results = _execute(args, prepared_data_reader)
    _write_results(args, results)


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
        args,
        metering_point_time_series,
        grid_loss_responsible_df,
    )

    if (
        args.calculation_type == CalculationType.WHOLESALE_FIXING
        or args.calculation_type == CalculationType.FIRST_CORRECTION_SETTLEMENT
        or args.calculation_type == CalculationType.SECOND_CORRECTION_SETTLEMENT
        or args.calculation_type == CalculationType.THIRD_CORRECTION_SETTLEMENT
    ):
        with logging_configuration.start_span("calculation.wholesale.prepare"):
            charge_master_data = prepared_data_reader.get_charge_master_data(
                args.calculation_period_start_datetime,
                args.calculation_period_end_datetime,
            )

            charge_prices = prepared_data_reader.get_charge_prices(
                args.calculation_period_start_datetime,
                args.calculation_period_end_datetime,
            )

            metering_points_periods_for_wholesale_calculation = (
                get_metering_points_and_child_metering_points(metering_point_periods_df)
            )

            charges_link_metering_point_periods = (
                prepared_data_reader.get_charge_link_metering_point_periods(
                    args.calculation_period_start_datetime,
                    args.calculation_period_end_datetime,
                    metering_points_periods_for_wholesale_calculation,
                )
            )

            wholesale_metering_point_time_series = (
                _get_wholesale_metering_point_times_series(
                    metering_point_time_series,
                    results.energy_results.positive_grid_loss,
                    results.energy_results.negative_grid_loss,
                )

            prepared_subscriptions = prepared_data_reader.get_subscription_charges(
                charge_master_data,
                charge_prices,
                charges_link_metering_point_periods,
                args.time_zone,
            )

            tariffs_hourly_df = prepared_data_reader.get_tariff_charges(
                wholesale_metering_point_time_series,
                charge_master_data,
                charge_prices,
                charges_link_metering_point_periods,
                ChargeResolution.HOUR,
                args.time_zone,
            )

            tariffs_daily_df = prepared_data_reader.get_tariff_charges(
                wholesale_metering_point_time_series,
                charge_master_data,
                charge_prices,
                charges_link_metering_point_periods,
                ChargeResolution.DAY,
                args.time_zone,
            )

        results.wholesale_results = wholesale_calculation.execute(
            args,
            prepared_subscriptions,
            tariffs_hourly_df,
            tariffs_daily_df,
        )

    # Add basis data to results
    results.basis_data = basis_data_factory.create(
        metering_point_periods_df, metering_point_time_series, args.time_zone
    )

    return results


@logging_configuration.use_span("calculation.write")
def _write_results(args: CalculatorArgs, results: CalculationResultsContainer) -> None:
    write_energy_results(results.energy_results)
    if results.wholesale_results is not None:
        write_wholesale_results(results.wholesale_results)

    # We write basis data at the end of the calculation to make it easier to analyze performance of the calculation part
    write_basis_data(args, results.basis_data)


def _get_wholesale_metering_point_times_series(
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

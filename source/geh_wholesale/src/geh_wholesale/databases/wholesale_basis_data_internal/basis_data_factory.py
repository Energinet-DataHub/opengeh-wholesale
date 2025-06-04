from geh_common.telemetry import use_span
from pyspark.sql import DataFrame

from geh_wholesale.calculation.calculation_output import BasisDataOutput
from geh_wholesale.calculation.calculator_args import CalculatorArgs
from geh_wholesale.calculation.preparation.data_structures import InputChargesContainer
from geh_wholesale.calculation.preparation.data_structures.grid_loss_metering_point_ids import (
    GridLossMeteringPointIds,
)
from geh_wholesale.calculation.preparation.data_structures.prepared_metering_point_time_series import (
    PreparedMeteringPointTimeSeries,
)
from geh_wholesale.databases.wholesale_basis_data_internal import basis_data


@use_span("calculation.basis_data.prepare")
def create(
    args: CalculatorArgs,
    metering_point_periods_df: DataFrame,
    metering_point_time_series_df: PreparedMeteringPointTimeSeries,
    grid_loss_metering_point_ids: GridLossMeteringPointIds,
    input_charges_container: InputChargesContainer | None = None,
) -> BasisDataOutput:
    time_series_points_basis_data = basis_data.get_time_series_points_basis_data(
        args.calculation_id, metering_point_time_series_df
    )

    metering_point_periods_basis_data = basis_data.get_metering_point_periods_basis_data(
        args.calculation_id, metering_point_periods_df
    )

    grid_loss_metering_point_ids_basis_data = basis_data.get_grid_loss_metering_point_ids_basis_data(
        args.calculation_id, grid_loss_metering_point_ids
    )

    if input_charges_container:
        charge_price_information_basis_data = basis_data.get_charge_price_information_basis_data(
            args.calculation_id, input_charges_container
        )

        charge_prices_basis_data = basis_data.get_charge_prices_basis_data(args.calculation_id, input_charges_container)

        charge_links_basis_data = basis_data.get_charge_links_basis_data(args.calculation_id, input_charges_container)
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
        grid_loss_metering_points=grid_loss_metering_point_ids_basis_data,
    )

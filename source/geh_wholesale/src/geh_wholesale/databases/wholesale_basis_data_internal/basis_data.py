import pyspark.sql.functions as f
from geh_common.telemetry import use_span
from pyspark.sql import DataFrame
from pyspark.sql.types import DecimalType

from geh_wholesale.calculation.preparation.data_structures import InputChargesContainer
from geh_wholesale.calculation.preparation.data_structures.grid_loss_metering_point_ids import (
    GridLossMeteringPointIds,
)
from geh_wholesale.calculation.preparation.data_structures.prepared_metering_point_time_series import (
    PreparedMeteringPointTimeSeries,
)
from geh_wholesale.constants import Colname
from geh_wholesale.databases.table_column_names import TableColumnNames


@use_span("get_metering_point_periods_basis_data")
def get_metering_point_periods_basis_data(
    calculation_id: str,
    metering_point_df: DataFrame,
) -> DataFrame:
    return metering_point_df.select(
        f.lit(calculation_id).alias(TableColumnNames.calculation_id),
        f.col(Colname.metering_point_id).alias(TableColumnNames.metering_point_id),
        f.col(Colname.metering_point_type).alias(TableColumnNames.metering_point_type),
        f.col(Colname.settlement_method).alias(TableColumnNames.settlement_method),
        f.col(Colname.grid_area_code).alias(TableColumnNames.grid_area_code),
        f.col(Colname.resolution).alias(TableColumnNames.resolution),
        f.col(Colname.from_grid_area_code).alias(TableColumnNames.from_grid_area_code),
        f.col(Colname.to_grid_area_code).alias(TableColumnNames.to_grid_area_code),
        f.col(Colname.parent_metering_point_id).alias(TableColumnNames.parent_metering_point_id),
        f.col(Colname.energy_supplier_id).alias(TableColumnNames.energy_supplier_id),
        f.col(Colname.balance_responsible_party_id).alias(TableColumnNames.balance_responsible_party_id),
        f.col(Colname.from_date).alias(TableColumnNames.from_date),
        f.col(Colname.to_date).alias(TableColumnNames.to_date),
    )


@use_span("get_time_series_points_basis_data")
def get_time_series_points_basis_data(
    calculation_id: str,
    metering_point_time_series: PreparedMeteringPointTimeSeries,
) -> DataFrame:
    return metering_point_time_series.df.select(
        f.lit(calculation_id).alias(TableColumnNames.calculation_id),
        f.col(Colname.metering_point_id).alias(TableColumnNames.metering_point_id),
        f.col(Colname.quantity).alias(TableColumnNames.quantity).cast(DecimalType(18, 3)),
        f.col(Colname.quality).alias(TableColumnNames.quality),
        f.col(Colname.observation_time).alias(TableColumnNames.observation_time),
        f.col(Colname.metering_point_type).alias(TableColumnNames.metering_point_type),
        f.col(Colname.resolution).alias(TableColumnNames.resolution),
        f.col(Colname.grid_area_code).alias(TableColumnNames.grid_area_code),
        f.col(Colname.energy_supplier_id).alias(TableColumnNames.energy_supplier_id),
    )


@use_span("get_charge_price_information_basis_data")
def get_charge_price_information_basis_data(
    calculation_id: str,
    input_charges_container: InputChargesContainer,
) -> DataFrame:
    return input_charges_container.charge_price_information._df.select(
        f.lit(calculation_id).alias(TableColumnNames.calculation_id),
        f.col(Colname.charge_key).alias(TableColumnNames.charge_key),
        f.col(Colname.charge_code).alias(TableColumnNames.charge_code),
        f.col(Colname.charge_type).alias(TableColumnNames.charge_type),
        f.col(Colname.charge_owner).alias(
            TableColumnNames.charge_owner_id,
        ),
        f.col(Colname.resolution).alias(TableColumnNames.resolution),
        f.col(Colname.charge_tax).alias(TableColumnNames.is_tax),
        f.col(Colname.from_date).alias(TableColumnNames.from_date),
        f.col(Colname.to_date).alias(TableColumnNames.to_date),
    )


@use_span("get_charge_price_points_basis_data")
def get_charge_prices_basis_data(
    calculation_id: str,
    input_charges_container: InputChargesContainer,
) -> DataFrame:
    return input_charges_container.charge_prices._df.select(
        f.lit(calculation_id).alias(TableColumnNames.calculation_id),
        f.col(Colname.charge_key).alias(TableColumnNames.charge_key),
        f.col(Colname.charge_code).alias(TableColumnNames.charge_code),
        f.col(Colname.charge_type).alias(TableColumnNames.charge_type),
        f.col(Colname.charge_owner).alias(TableColumnNames.charge_owner_id),
        f.col(Colname.charge_price).alias(TableColumnNames.charge_price),
        f.col(Colname.charge_time).alias(TableColumnNames.charge_time),
    )


@use_span("get_charge_link_periods_basis_data")
def get_charge_links_basis_data(
    calculation_id: str,
    input_charges_container: InputChargesContainer,
) -> DataFrame:
    return input_charges_container.charge_links.select(
        f.lit(calculation_id).alias(TableColumnNames.calculation_id),
        f.col(Colname.charge_key).alias(TableColumnNames.charge_key),
        f.col(Colname.charge_code).alias(TableColumnNames.charge_code),
        f.col(Colname.charge_type).alias(TableColumnNames.charge_type),
        f.col(Colname.charge_owner).alias(TableColumnNames.charge_owner_id),
        f.col(Colname.metering_point_id).alias(TableColumnNames.metering_point_id),
        f.col(Colname.quantity).alias(TableColumnNames.quantity),
        f.col(Colname.from_date).alias(TableColumnNames.from_date),
        f.col(Colname.to_date).alias(TableColumnNames.to_date),
    )


@use_span("get_grid_loss_metering_point_ids_basis_data")
def get_grid_loss_metering_point_ids_basis_data(
    calculation_id: str,
    grid_loss_metering_point_ids: GridLossMeteringPointIds,
) -> DataFrame:
    return grid_loss_metering_point_ids.df.select(
        f.lit(calculation_id).alias(TableColumnNames.calculation_id),
        f.col(Colname.metering_point_id).alias(TableColumnNames.metering_point_id),
    )

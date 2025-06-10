from pyspark.sql import DataFrame
from pyspark.sql.functions import col

from geh_wholesale.calculation.calculator_args import CalculatorArgs
from geh_wholesale.calculation.wholesale.data_structures.wholesale_results import (
    WholesaleResults,
)
from geh_wholesale.constants import Colname
from geh_wholesale.databases.table_column_names import TableColumnNames
from geh_wholesale.databases.wholesale_results_internal.add_meta_data import add_metadata
from geh_wholesale.infrastructure.paths import WholesaleResultsInternalDatabase


def create(args: CalculatorArgs, wholesale_results: WholesaleResults) -> DataFrame:
    wholesale_results = add_metadata(
        args,
        _get_column_group_for_calculation_result_id(),
        wholesale_results.df,
        WholesaleResultsInternalDatabase.AMOUNTS_PER_CHARGE_TABLE_NAME,
    )
    wholesale_results = _select_output_columns(wholesale_results)

    return wholesale_results


def _select_output_columns(df: DataFrame) -> DataFrame:
    # Map column names to the Delta table field names
    # Note: The order of the columns must match the order of the columns in the Delta table
    return df.select(
        col(Colname.calculation_id).alias(TableColumnNames.calculation_id),
        col(Colname.result_id).alias(TableColumnNames.result_id),
        col(Colname.grid_area_code).alias(TableColumnNames.grid_area_code),
        col(Colname.energy_supplier_id).alias(TableColumnNames.energy_supplier_id),
        col(Colname.total_quantity).alias(TableColumnNames.quantity),
        col(Colname.unit).alias(TableColumnNames.quantity_unit),
        col(Colname.qualities).alias(TableColumnNames.quantity_qualities),
        col(Colname.charge_time).alias(TableColumnNames.time),
        col(Colname.resolution).alias(TableColumnNames.resolution),
        col(Colname.metering_point_type).alias(TableColumnNames.metering_point_type),
        col(Colname.settlement_method).alias(TableColumnNames.settlement_method),
        col(Colname.charge_price).alias(TableColumnNames.price),
        col(Colname.total_amount).alias(TableColumnNames.amount),
        col(Colname.charge_tax).alias(TableColumnNames.is_tax),
        col(Colname.charge_code).alias(TableColumnNames.charge_code),
        col(Colname.charge_type).alias(TableColumnNames.charge_type),
        col(Colname.charge_owner).alias(TableColumnNames.charge_owner_id),
    )


def _get_column_group_for_calculation_result_id() -> list[str]:
    return [
        Colname.calculation_id,
        Colname.resolution,
        Colname.charge_type,
        Colname.charge_owner,
        Colname.charge_code,
        Colname.grid_area_code,
        Colname.energy_supplier_id,
        Colname.metering_point_type,
        Colname.settlement_method,
    ]

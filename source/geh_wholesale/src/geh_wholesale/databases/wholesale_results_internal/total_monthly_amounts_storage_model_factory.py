from pyspark.sql import DataFrame
from pyspark.sql.functions import col

from geh_wholesale.calculation.calculator_args import CalculatorArgs
from geh_wholesale.calculation.wholesale.data_structures import TotalMonthlyAmount
from geh_wholesale.constants import Colname
from geh_wholesale.databases.table_column_names import TableColumnNames
from geh_wholesale.databases.wholesale_results_internal.add_meta_data import add_metadata
from geh_wholesale.infrastructure.paths import WholesaleResultsInternalDatabase


def create(args: CalculatorArgs, total_monthly_amounts: TotalMonthlyAmount) -> DataFrame:
    total_monthly_amounts = add_metadata(
        args,
        _get_column_group_for_calculation_result_id(),
        total_monthly_amounts.df,
        WholesaleResultsInternalDatabase.TOTAL_MONTHLY_AMOUNTS_TABLE_NAME,
    )
    total_monthly_amounts = _select_output_columns(total_monthly_amounts)

    return total_monthly_amounts


def _select_output_columns(df: DataFrame) -> DataFrame:
    # Map column names to the Delta table field names
    # Note: The order of the columns must match the order of the columns in the Delta table
    return df.select(
        col(Colname.calculation_id).alias(TableColumnNames.calculation_id),
        col(Colname.result_id).alias(TableColumnNames.result_id),
        col(Colname.grid_area_code).alias(TableColumnNames.grid_area_code),
        col(Colname.energy_supplier_id).alias(TableColumnNames.energy_supplier_id),
        col(Colname.charge_time).alias(TableColumnNames.time),
        col(Colname.total_amount).alias(TableColumnNames.amount),
        col(Colname.charge_owner).alias(TableColumnNames.charge_owner_id),
    )


def _get_column_group_for_calculation_result_id() -> list[str]:
    return [
        Colname.calculation_id,
        Colname.charge_owner,
        Colname.grid_area_code,
        Colname.energy_supplier_id,
    ]

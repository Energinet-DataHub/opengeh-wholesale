import pyspark.sql.functions as f
from pyspark.sql import DataFrame
from pyspark.sql.types import DecimalType

from geh_wholesale.calculation.calculator_args import CalculatorArgs
from geh_wholesale.calculation.energy.data_structures.energy_results import (
    EnergyResults,
)
from geh_wholesale.calculation.energy.resolution_transition_factory import (
    get_energy_result_resolution,
)
from geh_wholesale.codelists import TimeSeriesType
from geh_wholesale.constants import Colname
from geh_wholesale.databases.table_column_names import TableColumnNames
from geh_wholesale.databases.wholesale_results_internal.add_meta_data import add_metadata
from geh_wholesale.infrastructure.paths import WholesaleResultsInternalDatabase


def create(
    args: CalculatorArgs,
    energy_results: EnergyResults,
    time_series_type: TimeSeriesType,
) -> DataFrame:
    df = _add_aggregation_level_and_time_series_type(energy_results.df, time_series_type)
    df = add_metadata(
        args,
        _get_column_group_for_calculation_result_id(),
        df,
        WholesaleResultsInternalDatabase.ENERGY_TABLE_NAME,
    )
    metering_point_resolution = get_energy_result_resolution(
        args.quarterly_resolution_transition_datetime,
        args.period_end_datetime,
    )
    df = df.withColumn(Colname.resolution, f.lit(metering_point_resolution.value))
    df = _map_to_storage_dataframe(df)

    return df


def _add_aggregation_level_and_time_series_type(
    results: DataFrame,
    time_series_type: TimeSeriesType,
) -> DataFrame:
    return results.withColumn(TableColumnNames.time_series_type, f.lit(time_series_type.value))


def _get_column_group_for_calculation_result_id() -> list[str]:
    """Get the columns that are required in order to define a single calculation result."""
    return [
        Colname.calculation_id,
        Colname.grid_area_code,
        Colname.from_grid_area_code,
        Colname.balance_responsible_party_id,
        Colname.energy_supplier_id,
        TableColumnNames.time_series_type,
    ]


def _map_to_storage_dataframe(results: DataFrame) -> DataFrame:
    """Map column names to the Delta table field names.

    Note: The order of the columns must match the order of the columns in the Delta table
    """
    return results.select(
        f.col(Colname.grid_area_code).alias(TableColumnNames.grid_area_code),
        f.col(Colname.energy_supplier_id).alias(TableColumnNames.energy_supplier_id),
        f.col(Colname.balance_responsible_party_id).alias(TableColumnNames.balance_responsible_party_id),
        f.col(Colname.quantity).alias(TableColumnNames.quantity).cast(DecimalType(18, 3)),
        f.col(Colname.qualities).alias(TableColumnNames.quantity_qualities),
        f.col(Colname.observation_time).alias(TableColumnNames.time),
        f.col(TableColumnNames.time_series_type),
        f.col(TableColumnNames.calculation_id),
        f.col(TableColumnNames.calculation_type),
        f.col(TableColumnNames.calculation_execution_time_start),
        f.col(Colname.from_grid_area_code).alias(TableColumnNames.neighbor_grid_area_code),
        f.col(TableColumnNames.result_id),
        f.col(TableColumnNames.metering_point_id),
        f.col(TableColumnNames.resolution),
    )

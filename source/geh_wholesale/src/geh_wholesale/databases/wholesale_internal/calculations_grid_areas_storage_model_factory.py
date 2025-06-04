from dependency_injector.wiring import Provide
from pyspark.sql import DataFrame, SparkSession

from geh_wholesale.calculation.calculator_args import CalculatorArgs
from geh_wholesale.container import Container
from geh_wholesale.databases.wholesale_internal.schemas.calculation_grid_areas_schema import (
    calculation_grid_areas_schema,
)


def create_calculation_grid_areas(
    args: CalculatorArgs,
) -> DataFrame:
    """Create a data frame containing calculation_id and grid area code."""
    return _create_calculation_grid_areas(args)


def _create_calculation_grid_areas(
    args: CalculatorArgs,
    spark: SparkSession = Provide[Container.spark],
) -> DataFrame:
    return spark.createDataFrame(
        [(args.calculation_id, grid_area_code) for grid_area_code in args.grid_areas],
        calculation_grid_areas_schema,
    )

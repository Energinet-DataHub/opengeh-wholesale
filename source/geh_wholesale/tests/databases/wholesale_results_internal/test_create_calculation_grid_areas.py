from pyspark.sql import Row, SparkSession

from geh_wholesale.calculation.calculator_args import CalculatorArgs
from geh_wholesale.databases.wholesale_internal.calculations_grid_areas_storage_model_factory import (
    create_calculation_grid_areas,
)
from geh_wholesale.databases.wholesale_internal.schemas.calculation_grid_areas_schema import (
    calculation_grid_areas_schema,
)
from tests.helpers.data_frame_utils import assert_dataframes_equal


def test__when_valid_input__creates_calculation_grid_areas_with_expected_schema(
    any_calculator_args: CalculatorArgs, spark: SparkSession
) -> None:
    # Arrange
    row1 = Row(
        calculation_id=any_calculator_args.calculation_id,
        grid_area_code=any_calculator_args.grid_areas[0],
    )
    row2 = Row(
        calculation_id=any_calculator_args.calculation_id,
        grid_area_code=any_calculator_args.grid_areas[1],
    )
    expected = spark.createDataFrame([row1, row2], calculation_grid_areas_schema)

    # Act
    actual = create_calculation_grid_areas(any_calculator_args)

    # Assert
    assert_dataframes_equal(actual, expected)

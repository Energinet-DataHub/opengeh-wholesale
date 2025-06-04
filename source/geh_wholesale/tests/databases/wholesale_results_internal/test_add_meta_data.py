from pyspark import Row
from pyspark.sql import SparkSession

from geh_wholesale.calculation.calculator_args import CalculatorArgs
from geh_wholesale.constants import Colname
from geh_wholesale.databases.table_column_names import TableColumnNames
from geh_wholesale.databases.wholesale_internal.schemas import calculation_grid_areas_schema
from geh_wholesale.databases.wholesale_results_internal.add_meta_data import (
    _add_calculation_result_id,
)


def test__added_calculation_result_id_is_correct(
    any_calculator_args: CalculatorArgs,
    spark: SparkSession,
) -> None:
    """
    Test that the generated result_id is the same every time given the same input.
    """
    # Arrange
    result_id = "c2853ac9-632e-5bef-864b-5468a7000ca2"

    row1 = Row(
        calculation_id=any_calculator_args.calculation_id,
        grid_area_code=any_calculator_args.grid_areas[0],
    )
    df = spark.createDataFrame([row1], calculation_grid_areas_schema)

    # Act
    expected = _add_calculation_result_id(
        df,
        [
            Colname.calculation_id,
            Colname.grid_area_code,
        ],
        "test_table",
    )

    # Assert
    assert TableColumnNames.result_id in expected.columns
    assert expected.collect()[0].result_id == result_id

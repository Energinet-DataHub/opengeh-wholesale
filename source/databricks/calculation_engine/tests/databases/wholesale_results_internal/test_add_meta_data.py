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
from pyspark import Row
from pyspark.sql import SparkSession

from package.calculation.calculator_args import CalculatorArgs
from package.constants import Colname
from package.databases.table_column_names import TableColumnNames
from package.databases.wholesale_internal.schemas import calculation_grid_areas_schema
from package.databases.wholesale_results_internal.add_meta_data import (
    _add_calculation_result_id,
)


def test__added_calculation_result_id_is_correct(
    any_calculator_args: CalculatorArgs,
    spark: SparkSession,
) -> None:
    """
    Test that the generated calculation_result_id is the same every time given the same input.
    """
    # Arrange
    calculation_result_id = "c2853ac9-632e-5bef-864b-5468a7000ca2"

    row1 = Row(
        calculation_id=any_calculator_args.calculation_id,
        grid_area_code=any_calculator_args.calculation_grid_areas[0],
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
    assert TableColumnNames.calculation_result_id in expected.columns
    assert expected.collect()[0].calculation_result_id == calculation_result_id

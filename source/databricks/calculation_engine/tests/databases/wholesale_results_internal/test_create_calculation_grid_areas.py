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

from pyspark.sql import Row, SparkSession

from tests.helpers.data_frame_utils import assert_dataframes_equal
from package.calculation.calculator_args import CalculatorArgs
from package.databases.wholesale_internal.schemas.calculation_grid_areas_schema import (
    calculation_grid_areas_schema,
)
from package.databases.wholesale_internal.calculations_grid_areas_storage_model_factory import (
    create_calculation_grid_areas,
)


def test__when_valid_input__creates_calculation_grid_areas_with_expected_schema(
    any_calculator_args: CalculatorArgs, spark: SparkSession
) -> None:
    # Arrange
    row1 = Row(
        calculation_id=any_calculator_args.calculation_id,
        grid_area_code=any_calculator_args.calculation_grid_areas[0],
    )
    row2 = Row(
        calculation_id=any_calculator_args.calculation_id,
        grid_area_code=any_calculator_args.calculation_grid_areas[1],
    )
    expected = spark.createDataFrame([row1, row2], calculation_grid_areas_schema)

    # Act
    actual = create_calculation_grid_areas(any_calculator_args)

    # Assert
    assert_dataframes_equal(actual, expected)

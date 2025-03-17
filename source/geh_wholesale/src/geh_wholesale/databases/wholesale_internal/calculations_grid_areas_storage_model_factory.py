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
    """Creates a data frame containing calculation_id and grid area code."""
    return _create_calculation_grid_areas(args)


def _create_calculation_grid_areas(
    args: CalculatorArgs,
    spark: SparkSession = Provide[Container.spark],
) -> DataFrame:
    return spark.createDataFrame(
        [(args.calculation_id, grid_area_code) for grid_area_code in args.grid_areas],
        calculation_grid_areas_schema,
    )

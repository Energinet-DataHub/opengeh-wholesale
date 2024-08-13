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

from pyspark.sql import SparkSession

from databases.wholesale_basis_data_internal.basis_data_test_factory import (
    create_calculation_args,
)
from databases.wholesale_results_internal.calculations_storage_model_test_factory import (
    create_calculations,
)
from package.calculation.calculation_results import InternalContainer
from package.databases.wholesale_basis_data_internal import internal_factory
from package.databases.wholesale_results_internal.calculations_grid_areas_storage_model_factory import (
    create_calculation_grid_areas,
)


def create_internal_factory(spark: SparkSession) -> InternalContainer:
    calculations = create_calculations(spark)
    calculation_grid_areas = create_calculation_grid_areas(create_calculation_args())

    return internal_factory.create(
        calculations=calculations,
        calculation_grid_areas=calculation_grid_areas,
    )

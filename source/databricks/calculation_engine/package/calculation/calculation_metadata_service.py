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
from .calculator_args import CalculatorArgs
from ..databases.wholesale_internal.calculation_writer import (
    write_calculation,
    write_calculation_grid_areas,
    write_calculation_succeeded_time,
)
from ..databases.wholesale_internal.calculations_grid_areas_storage_model_factory import (
    create_calculation_grid_areas,
)


class CalculationMetadataService:

    @staticmethod
    def write(args: CalculatorArgs) -> None:
        write_calculation(args)

        calculation_grid_areas = create_calculation_grid_areas(args)
        write_calculation_grid_areas(calculation_grid_areas)

    @staticmethod
    def write_calculation_succeeded_time(calculation_id: str) -> None:
        write_calculation_succeeded_time(calculation_id)

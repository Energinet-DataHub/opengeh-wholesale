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
from typing import Any

from package.calculation.wholesale.links.calculation_step import CalculationLink

from package.calculation.calculation_output import CalculationOutput


class ExampleStep(CalculationLink):

    def __init__(
        self,
        # Injections like configurations, repositories, services, etc.
        injections: Any,
    ):
        super().__init__()

        # Guard clauses
        if injections is None:
            raise ValueError("injections cannot be None")

        # Does input conform to business rules?
        if injections is None:
            raise ValueError("injections are invalid")

        # Assignments
        self.injections = injections

    def execute(self, output: CalculationOutput) -> CalculationOutput:

        # Business Logic
        output.x = ""

        # Pass calculation output to the next step for execution
        return super().execute(output)

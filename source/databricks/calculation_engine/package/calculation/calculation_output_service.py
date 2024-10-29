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

from .calculation_output import (
    CalculationOutput,
)
import package.databases.wholesale_basis_data_internal as basis_data_database
import package.databases.wholesale_results_internal as result_database


class CalculationOutputService:

    @staticmethod
    def write(
        calculation_output: CalculationOutput,
    ) -> None:
        result_database.write_energy_results(calculation_output.energy_results_output)
        if calculation_output.wholesale_results_output is not None:
            result_database.write_wholesale_results(
                calculation_output.wholesale_results_output
            )
            result_database.write_monthly_amounts_per_charge(
                calculation_output.wholesale_results_output
            )
            result_database.write_total_monthly_amounts(
                calculation_output.wholesale_results_output
            )

        if calculation_output.basis_data_output is not None:
            # We write basis data at the end of the calculation to make it easier to analyze performance of the calculation part
            basis_data_database.write_basis_data(calculation_output.basis_data_output)

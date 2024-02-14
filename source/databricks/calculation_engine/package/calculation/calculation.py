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

from package.calculation.calculation_execute import calculation_execute
from .CalculationResults import (
    CalculationResultsContainer,
)
from .calculator_args import CalculatorArgs
from .output.basis_data import write_basis_data
from .output.energy_results import write_energy_results
from .output.wholesale_results import write_wholesale_results
from .preparation import PreparedDataReader


def execute(args: CalculatorArgs, prepared_data_reader: PreparedDataReader) -> None:
    results = calculation_execute(args, prepared_data_reader)
    _write_results(args, results)


def _write_results(args: CalculatorArgs, results: CalculationResultsContainer) -> None:
    write_energy_results(args, results.energy_results)
    if results.wholesale_results is not None:
        write_wholesale_results(args, results.wholesale_results)
    # We write basis data at the end of the calculation to make it easier to analyze performance of the calculation part
    write_basis_data(args, results.basis_data)

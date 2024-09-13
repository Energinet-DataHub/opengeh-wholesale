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
from .calculation_core import CalculationCore
from .calculation_metadata_service import CalculationMetadataService
from .calculation_output_service import CalculationOutputService
from .calculator_args import CalculatorArgs
from .preparation import PreparedDataReader


def execute(
    args: CalculatorArgs,
    prepared_data_reader: PreparedDataReader,
    calculation_core: CalculationCore,
    calculation_metadata_service: CalculationMetadataService,
    calculation_output_service: CalculationOutputService,
) -> None:
    calculation_metadata_service.write(args)

    output = calculation_core.execute(args, prepared_data_reader)

    calculation_output_service.write(output)

    # IMPORTANT: Write the succeeded calculation after the results to ensure that the calculation
    # is only marked as succeeded when all results are written
    calculation_metadata_service.write_calculation_succeeded_time(args.calculation_id)

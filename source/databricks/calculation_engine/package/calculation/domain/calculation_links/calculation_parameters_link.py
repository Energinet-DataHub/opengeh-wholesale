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


from package.calculation import PreparedDataReader
from package.calculation.calculation_output import CalculationOutput
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.domain.chains.calculation_link import BaseCalculationLink
from package.calculation.wholesale.links.metering_point_period_repository import (
    CalculationMetaData,
)


class CreateCalculationMetaDataOutputStep(BaseCalculationLink):

    def __init__(
        self,
        calculator_args: CalculatorArgs,
        prepared_data_reader: PreparedDataReader,
    ):
        super().__init__()
        self.calculator_args = calculator_args
        self.prepared_data_reader = prepared_data_reader

    def handle(self, output: CalculationOutput) -> CalculationOutput:

        latest_version = self.prepared_data_reader.get_latest_calculation_version(
            self.calculator_args.calculation_type
        )

        # Next version begins with 1 and increments by 1
        next_version = (latest_version or 0) + 1

        output.calculation_meta_data = CalculationMetaData(self.calculator_args)

        return output

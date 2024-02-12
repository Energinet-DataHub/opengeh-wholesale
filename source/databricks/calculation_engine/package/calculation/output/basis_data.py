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
from package.calculation.CalculationResults import BasisDataContainer
from package.calculation.calculator_args import CalculatorArgs
from package.calculation_output.basis_data_writer import BasisDataWriter


def write_basis_data(args: CalculatorArgs, basis_data: BasisDataContainer):
    basis_data_writer = BasisDataWriter(
        args.wholesale_container_path, args.calculation_id
    )
    basis_data_writer.write(
        basis_data.metering_point_periods,
        basis_data.metering_point_time_series,
        args.time_zone,
    )

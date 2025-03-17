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
from unittest.mock import Mock, patch

import pytest
from pydantic_core import ValidationError

from geh_wholesale.calculation.calculator_args import CalculatorArgs
from geh_wholesale.calculator_job import start, start_with_deps
from geh_wholesale.infrastructure.infrastructure_settings import InfrastructureSettings


class TestWhenInvokedWithInvalidArguments:
    def test_exits_with_code_2(self) -> None:
        """The exit code 2 originates from the argparse library."""
        with pytest.raises(ValidationError):
            start()


class TestWhenInvokedWithValidArguments:
    def test_does_not_raise(
        self,
        any_calculator_args: CalculatorArgs,
        infrastructure_settings: InfrastructureSettings,
    ) -> None:
        mock_calculation_execute = Mock()
        mock_prepared_data_reader = Mock()
        mock_prepared_data_reader.is_calculation_id_unique.return_value = True

        with patch("package.calculation.execute", mock_calculation_execute):
            with patch(
                "package.calculation.PreparedDataReader",
                return_value=mock_prepared_data_reader,
            ):
                # Act
                start_with_deps(
                    args=any_calculator_args,
                    infrastructure_settings=infrastructure_settings,
                )

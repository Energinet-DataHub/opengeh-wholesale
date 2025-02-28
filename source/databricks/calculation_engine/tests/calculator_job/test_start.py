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
import argparse
import sys
import time
import uuid
from datetime import timedelta
from typing import cast, Callable
from unittest.mock import Mock, patch

import pytest
from azure.monitor.query import LogsQueryClient, LogsQueryResult

from package.calculation.calculator_args import CalculatorArgs
from package.calculator_job import start, start_with_deps
from package.infrastructure.infrastructure_settings import InfrastructureSettings
from tests.integration_test_configuration import IntegrationTestConfiguration
from pydantic_core import ValidationError
import geh_common.telemetry.logging_configuration as config
import os


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

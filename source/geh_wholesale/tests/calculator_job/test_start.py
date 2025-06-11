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
        mock_feature_manager = Mock()
        mock_prepared_data_reader.is_calculation_id_unique.return_value = True

        with patch("geh_wholesale.calculation.execute", mock_calculation_execute):
            with patch(
                "geh_wholesale.calculation.PreparedDataReader",
                return_value=mock_prepared_data_reader,
            ):
                with patch(
                    "geh_wholesale.calculator_job.create_feature_manager",
                    return_value=mock_feature_manager,
                ):
                    # Act
                    start_with_deps(
                        args=any_calculator_args,
                        infrastructure_settings=infrastructure_settings,
                    )

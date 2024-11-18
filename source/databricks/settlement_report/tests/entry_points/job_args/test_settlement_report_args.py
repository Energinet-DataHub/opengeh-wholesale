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
import re
import uuid
from datetime import datetime
from unittest.mock import patch

import pytest

from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.entry_points.entry_point import (
    parse_job_arguments,
    parse_command_line_arguments,
)

from settlement_report_job.entry_points.job_args.environment_variables import (
    EnvironmentVariable,
)
from settlement_report_job.entry_points.job_args.calculation_type import CalculationType

DEFAULT_REPORT_ID = "12345678-9fc8-409a-a169-fbd49479d718"


def _get_contract_parameters(filename: str) -> list[str]:
    """Get the parameters as they are expected to be received from the settlement report invoker."""  # noqa
    with open(filename) as file:
        text = file.read()
        text = text.replace("{report-id}", DEFAULT_REPORT_ID)
        lines = text.splitlines()
        return list(
            filter(lambda line: not line.startswith("#") and len(line) > 0, lines)
        )


def _substitute_requesting_actor_market_role(
    sys_argv: list[str], market_role: str
) -> list[str]:
    pattern = r"--requesting-actor-market-role=(\w+)"

    for i, item in enumerate(sys_argv):
        if re.search(pattern, item):
            sys_argv[i] = re.sub(
                pattern, f"--requesting-actor-market-role={market_role}", item
            )
            break

    return sys_argv


def _substitute_energy_supplier_ids(
    sys_argv: list[str], energy_supplier_ids: str
) -> list[str]:
    for i, item in enumerate(sys_argv):
        if item.startswith("--energy-supplier-ids="):
            sys_argv[i] = f"--energy-supplier-ids={energy_supplier_ids}"
            break
    return sys_argv


@pytest.fixture(scope="session")
def contract_parameters_for_balance_fixing(contracts_path: str) -> list[str]:
    job_parameters = _get_contract_parameters(
        f"{contracts_path}/settlement-report-balance-fixing-parameters-reference.txt"
    )

    return job_parameters


@pytest.fixture(scope="session")
def contract_parameters_for_wholesale(contracts_path: str) -> list[str]:
    job_parameters = _get_contract_parameters(
        f"{contracts_path}/settlement-report-wholesale-calculations-parameters-reference.txt"
    )

    return job_parameters


@pytest.fixture(scope="session")
def sys_argv_from_contract_for_wholesale(
    contract_parameters_for_wholesale: list[str],
) -> list[str]:
    return ["dummy_script_name"] + contract_parameters_for_wholesale


@pytest.fixture(scope="session")
def sys_argv_from_contract_for_balance_fixing(
    contract_parameters_for_balance_fixing: list[str],
) -> list[str]:
    return ["dummy_script_name"] + contract_parameters_for_balance_fixing


@pytest.fixture(scope="session")
def job_environment_variables() -> dict:
    return {
        EnvironmentVariable.CATALOG_NAME.name: "some_catalog",
    }


def test_when_invoked_with_incorrect_parameters__fails(
    job_environment_variables: dict,
) -> None:
    # Arrange
    with pytest.raises(SystemExit) as excinfo:
        with patch("sys.argv", ["dummy_script", "--unexpected-arg"]):
            with patch.dict("os.environ", job_environment_variables):
                # Act
                parse_command_line_arguments()

    # Assert
    assert excinfo.value.code == 2


def test_when_parameters_for_balance_fixing__parses_parameters_from_contract(
    job_environment_variables: dict,
    sys_argv_from_contract_for_balance_fixing: list[str],
) -> None:
    """
    This test ensures that the settlement report job for balance fixing accepts
    the arguments that are provided by the client.
    """
    # Arrange
    with patch("sys.argv", sys_argv_from_contract_for_balance_fixing):
        with patch.dict("os.environ", job_environment_variables):
            command_line_args = parse_command_line_arguments()
            # Act
            actual_args = parse_job_arguments(command_line_args)

    # Assert - settlement report arguments
    assert actual_args.report_id == DEFAULT_REPORT_ID
    assert actual_args.period_start == datetime(2022, 5, 31, 22)
    assert actual_args.period_end == datetime(2022, 6, 1, 22)
    assert actual_args.calculation_type == CalculationType.BALANCE_FIXING
    assert actual_args.grid_area_codes == ["804", "805"]
    assert actual_args.energy_supplier_ids == ["1234567890123"]
    assert actual_args.prevent_large_text_files is True
    assert actual_args.split_report_by_grid_area is True
    assert actual_args.time_zone == "Europe/Copenhagen"
    assert actual_args.include_basis_data is True


def test_when_parameters_for_wholesale__parses_parameters_from_contract(
    job_environment_variables: dict,
    sys_argv_from_contract_for_wholesale: list[str],
) -> None:
    """
    This test ensures that the settlement report job for wholesale calculations accepts
    the arguments that are provided by the client.
    """
    # Arrange
    with patch("sys.argv", sys_argv_from_contract_for_wholesale):
        with patch.dict("os.environ", job_environment_variables):
            command_line_args = parse_command_line_arguments()
            # Act
            actual_args = parse_job_arguments(command_line_args)

    # Assert - settlement report arguments
    assert actual_args.report_id == DEFAULT_REPORT_ID
    assert actual_args.period_start == datetime(2022, 5, 31, 22)
    assert actual_args.period_end == datetime(2022, 6, 1, 22)
    assert actual_args.calculation_type == CalculationType.WHOLESALE_FIXING
    assert actual_args.calculation_id_by_grid_area == {
        "804": uuid.UUID("95bd2365-c09b-4ee7-8c25-8dd56b564811"),
        "805": uuid.UUID("d3e2b83a-2fd9-4bcd-a6dc-41e4ce74cd6d"),
    }
    assert actual_args.energy_supplier_ids == ["1234567890123"]
    assert actual_args.prevent_large_text_files is True
    assert actual_args.split_report_by_grid_area is True
    assert actual_args.time_zone == "Europe/Copenhagen"
    assert actual_args.include_basis_data is True


@pytest.mark.parametrize(
    "not_valid_calculation_id",
    [
        "not_valid",
        "",
        None,
        "c09b-4ee7-8c25-8dd56b564811",  # too short
    ],
)
def test_when_no_valid_calculation_id_for_grid_area__raises_uuid_value_error(
    job_environment_variables: dict,
    sys_argv_from_contract_for_wholesale: list[str],
    not_valid_calculation_id: str,
) -> None:
    # Arrange
    test_sys_args = sys_argv_from_contract_for_wholesale.copy()
    pattern = r"--calculation-id-by-grid-area=(\{.*\})"

    for i, item in enumerate(test_sys_args):
        if re.search(pattern, item):
            test_sys_args[i] = re.sub(
                pattern,
                f'--calculation-id-by-grid-area={{"804": "{not_valid_calculation_id}"}}',  # noqa
                item,
            )
            break

    with patch("sys.argv", test_sys_args):
        with patch.dict("os.environ", job_environment_variables):
            with pytest.raises(ValueError) as exc_info:
                command_line_args = parse_command_line_arguments()
                # Act
                parse_job_arguments(command_line_args)

    # Assert
    assert "Calculation ID for grid area 804 is not a uuid" in str(exc_info.value)


@pytest.mark.parametrize(
    "prevent_large_text_files",
    [
        True,
        False,
    ],
)
def test_returns_expected_value_for_prevent_large_text_files(
    job_environment_variables: dict,
    sys_argv_from_contract_for_wholesale: list[str],
    prevent_large_text_files: bool,
) -> None:
    # Arrange
    test_sys_args = sys_argv_from_contract_for_wholesale.copy()
    if not prevent_large_text_files:
        test_sys_args = [
            item
            for item in sys_argv_from_contract_for_wholesale
            if not item.startswith("--prevent-large-text-files")
        ]

    with patch("sys.argv", test_sys_args):
        with patch.dict("os.environ", job_environment_variables):
            command_line_args = parse_command_line_arguments()

            # Act
            actual_args = parse_job_arguments(command_line_args)

    # Assert
    assert actual_args.prevent_large_text_files is prevent_large_text_files


@pytest.mark.parametrize(
    "split_report_by_grid_area",
    [
        True,
        False,
    ],
)
def test_returns_expected_value_for_split_report_by_grid_area(
    job_environment_variables: dict,
    sys_argv_from_contract_for_wholesale: list[str],
    split_report_by_grid_area: bool,
) -> None:
    # Arrange
    test_sys_args = sys_argv_from_contract_for_wholesale.copy()
    if not split_report_by_grid_area:
        test_sys_args = [
            item
            for item in sys_argv_from_contract_for_wholesale
            if not item.startswith("--split-report-by-grid-area")
        ]

    with patch("sys.argv", test_sys_args):
        with patch.dict("os.environ", job_environment_variables):
            command_line_args = parse_command_line_arguments()

            # Act
            actual_args = parse_job_arguments(command_line_args)

    # Assert
    assert actual_args.split_report_by_grid_area is split_report_by_grid_area


@pytest.mark.parametrize(
    "include_basis_data",
    [
        True,
        False,
    ],
)
def test_returns_expected_value_for_include_basis_data(
    job_environment_variables: dict,
    sys_argv_from_contract_for_wholesale: list[str],
    include_basis_data: bool,
) -> None:
    # Arrange
    test_sys_args = sys_argv_from_contract_for_wholesale.copy()
    if not include_basis_data:
        test_sys_args = [
            item
            for item in sys_argv_from_contract_for_wholesale
            if not item.startswith("--include-basis-data")
        ]

    with patch("sys.argv", test_sys_args):
        with patch.dict("os.environ", job_environment_variables):
            command_line_args = parse_command_line_arguments()

            # Act
            actual_args = parse_job_arguments(command_line_args)

    # Assert
    assert actual_args.include_basis_data is include_basis_data


@pytest.mark.parametrize(
    "energy_supplier_ids_argument, expected_energy_suppliers_ids",
    [
        ("[1234567890123]", ["1234567890123"]),
        ("[1234567890123]", ["1234567890123"]),
        ("[1234567890123, 2345678901234]", ["1234567890123", "2345678901234"]),
        ("[1234567890123,2345678901234]", ["1234567890123", "2345678901234"]),
        ("[ 1234567890123,2345678901234 ]", ["1234567890123", "2345678901234"]),
    ],
)
def test_when_energy_supplier_ids_are_specified__returns_expected_energy_supplier_ids(
    sys_argv_from_contract_for_wholesale: list[str],
    job_environment_variables: dict,
    energy_supplier_ids_argument: str,
    expected_energy_suppliers_ids: list[str],
) -> None:
    # Arrange
    test_sys_args = sys_argv_from_contract_for_wholesale.copy()
    test_sys_args = _substitute_energy_supplier_ids(
        test_sys_args, energy_supplier_ids_argument
    )

    with patch.dict("os.environ", job_environment_variables):
        with patch("sys.argv", test_sys_args):
            command_line_args = parse_command_line_arguments()

            # Act
            actual_args = parse_job_arguments(command_line_args)

    # Assert
    assert actual_args.energy_supplier_ids == expected_energy_suppliers_ids


@pytest.mark.parametrize(
    "energy_supplier_ids_argument",
    [
        "1234567890123",  # not a list
        "1234567890123 2345678901234",  # not a list
        "[123]",  # neither 13 nor 16 characters
        "[12345678901234]",  # neither 13 nor 16 characters
    ],
)
def test_when_invalid_energy_supplier_ids__raise_exception(
    sys_argv_from_contract_for_wholesale: list[str],
    job_environment_variables: dict,
    energy_supplier_ids_argument: str,
) -> None:
    # Arrange
    test_sys_args = sys_argv_from_contract_for_wholesale.copy()
    test_sys_args = _substitute_energy_supplier_ids(
        test_sys_args, energy_supplier_ids_argument
    )

    with patch.dict("os.environ", job_environment_variables):
        with patch("sys.argv", test_sys_args):
            with pytest.raises(SystemExit) as error:
                command_line_args = parse_command_line_arguments()
                # Act
                parse_job_arguments(command_line_args)

    # Assert
    assert error.value.code != 0


def test_when_no_energy_supplier_specified__returns_none_energy_supplier_ids(
    sys_argv_from_contract_for_wholesale: list[str],
    job_environment_variables: dict,
) -> None:
    # Arrange
    test_sys_args = [
        item
        for item in sys_argv_from_contract_for_wholesale
        if not item.startswith("--energy-supplier-ids")
    ]

    with patch.dict("os.environ", job_environment_variables):
        with patch("sys.argv", test_sys_args):
            command_line_args = parse_command_line_arguments()

            # Act
            actual_args = parse_job_arguments(command_line_args)

    # Assert
    assert actual_args.energy_supplier_ids is None


class TestWhenInvokedWithValidMarketRole:
    @pytest.mark.parametrize(
        "market_role",
        [market_role for market_role in MarketRole],
    )
    def test_returns_expected_requesting_actor_market_role(
        self,
        job_environment_variables: dict,
        sys_argv_from_contract_for_wholesale: list[str],
        market_role: MarketRole,
    ) -> None:
        # Arrange
        test_sys_args = _substitute_requesting_actor_market_role(
            sys_argv_from_contract_for_wholesale.copy(), market_role.value
        )

        with patch("sys.argv", test_sys_args):
            with patch.dict("os.environ", job_environment_variables):
                command_line_args = parse_command_line_arguments()

                # Act
                actual_args = parse_job_arguments(command_line_args)

        # Assert
        assert actual_args.requesting_actor_market_role == market_role


class TestWhenInvokedWithInvalidMarketRole:

    def test_raise_system_exit_with_non_zero_code(
        self,
        job_environment_variables: dict,
        sys_argv_from_contract_for_wholesale: list[str],
    ) -> None:
        # Arrange
        test_sys_args = _substitute_requesting_actor_market_role(
            sys_argv_from_contract_for_wholesale.copy(), "invalid_market_role"
        )

        with patch("sys.argv", test_sys_args):
            with patch.dict("os.environ", job_environment_variables):
                with pytest.raises(SystemExit) as error:
                    command_line_args = parse_command_line_arguments()
                    # Act
                    parse_job_arguments(command_line_args)

        # Assert
        assert error.value.code != 0


class TestWhenUnknownCalculationType:
    def test_raise_system_exit_with_non_zero_code(
        self,
        job_environment_variables: dict,
        sys_argv_from_contract_for_wholesale: list[str],
    ) -> None:
        # Arrange
        test_sys_args = sys_argv_from_contract_for_wholesale.copy()
        unknown_calculation_type = "unknown_calculation_type"
        pattern = r"--calculation-type=(\w+)"

        for i, item in enumerate(test_sys_args):
            if re.search(pattern, item):
                test_sys_args[i] = re.sub(
                    pattern, f"--calculation-type={unknown_calculation_type}", item
                )
                break

        with patch("sys.argv", test_sys_args):
            with patch.dict("os.environ", job_environment_variables):
                with pytest.raises(SystemExit) as error:
                    command_line_args = parse_command_line_arguments()
                    # Act
                    parse_job_arguments(command_line_args)

        # Assert
        assert error.value.code != 0


class TestWhenMissingEnvVariables:
    def test_raise_system_exit_with_non_zero_code(
        self,
        job_environment_variables: dict,
        sys_argv_from_contract_for_wholesale: list[str],
    ) -> None:
        # Arrange
        with patch("sys.argv", sys_argv_from_contract_for_wholesale):
            for excluded_env_var in job_environment_variables.keys():
                env_variables_with_one_missing = {
                    key: value
                    for key, value in job_environment_variables.items()
                    if key != excluded_env_var
                }

                with patch.dict("os.environ", env_variables_with_one_missing):
                    with pytest.raises(ValueError) as error:
                        command_line_args = parse_command_line_arguments()
                        # Act
                        parse_job_arguments(command_line_args)

                assert str(error.value).startswith("Environment variable not found")

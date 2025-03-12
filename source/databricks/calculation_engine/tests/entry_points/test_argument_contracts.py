from datetime import datetime
import os
import sys
from dataclasses import dataclass
from pathlib import Path

import pydantic
import pytest


from package.calculation.calculator_args import CalculatorArgs
from tests import PROJECT_PATH

DEFAULT_CALCULATION_ID = "12345678-9fc8-409a-a169-fbd49479d718"


@dataclass
class Contract:
    required: list
    optional: list


def _load_contract(path: Path):
    lines = path.read_text().splitlines()
    required_params = []
    optional_params = []
    mode = None
    for line in lines:
        if "required parameters" in line.lower():
            mode = "required"
            continue
        if "optional parameters" in line.lower():
            mode = "optional"
            continue
        if mode == "required" and line.startswith("--"):
            required_params.append(
                line.replace("{calculation-id}", DEFAULT_CALCULATION_ID)
            )
        if mode == "optional" and line.startswith("--"):
            optional_params.append(line)
    return Contract(required_params, optional_params)


def _load_contracts():
    contract_path = PROJECT_PATH / "contracts"
    contracts = {}
    for p in contract_path.iterdir():
        if p.suffix != ".txt":
            continue
        contracts[p.stem] = _load_contract(p)
    return contracts


def _args_to_dict(args):
    args_dict = {}
    for arg in args:
        if not arg.startswith("--"):
            continue
        if "=" in arg:
            key, value = arg.split("=")
        else:
            key = arg
            value = True
        args_dict[key[2:]] = value
    return args_dict


def _assert_args(args: CalculatorArgs, args_dict, env_args):
    assert args.calculation_id == args_dict["calculation-id"]
    assert args.period_start_datetime == datetime.fromisoformat(
        args_dict["period-start-datetime"]
    )
    assert args.period_end_datetime == datetime.fromisoformat(
        args_dict["period-end-datetime"]
    )
    assert args.calculation_type.value == args_dict["calculation-type"]
    assert args.created_by_user_id == args_dict["created-by-user-id"]
    assert args.time_zone == env_args["TIME_ZONE"]
    assert args.quarterly_resolution_transition_datetime == datetime.fromisoformat(
        env_args["QUARTERLY_RESOLUTION_TRANSITION_DATETIME"]
    )


@pytest.fixture
def environment_variables():
    return {
        "TIME_ZONE": "Europe/Copenhagen",
        "QUARTERLY_RESOLUTION_TRANSITION_DATETIME": "2023-01-31T23:00:00Z",
    }


@pytest.mark.parametrize(["_", "contract"], _load_contracts().items())
def test_calculator_required_args(
    _, contract: Contract, environment_variables: dict, monkeypatch: pytest.MonkeyPatch
):
    args_dict = _args_to_dict(contract.required)
    monkeypatch.setattr(
        sys,
        "argv",
        ["calculator"] + contract.required,
    )
    monkeypatch.setattr(os, "environ", environment_variables)
    args = CalculatorArgs()
    _assert_args(args, args_dict, environment_variables)


@pytest.mark.parametrize(["_", "contract"], _load_contracts().items())
def test_calculator_optional_args(
    _, contract: Contract, environment_variables: dict, monkeypatch: pytest.MonkeyPatch
):
    args_dict = _args_to_dict(contract.required + contract.optional)
    monkeypatch.setattr(
        sys,
        "argv",
        ["calculator"] + contract.required + contract.optional,
    )
    monkeypatch.setattr(os, "environ", environment_variables)
    args = CalculatorArgs()
    _assert_args(args, args_dict, environment_variables)


@pytest.mark.parametrize(["_", "contract"], _load_contracts().items())
def test_calculator_args_missing_env(
    _, contract: Contract, environment_variables: dict, monkeypatch: pytest.MonkeyPatch
):
    environment_variables.pop("TIME_ZONE")
    monkeypatch.setattr(
        sys,
        "argv",
        ["calculator"] + contract.required + contract.optional,
    )
    monkeypatch.setattr(os, "environ", environment_variables)
    with pytest.raises(pydantic.ValidationError):
        CalculatorArgs()


@pytest.mark.parametrize(["_", "contract"], _load_contracts().items())
def test_calculator_args_required_args_missing(
    _, contract: Contract, environment_variables: dict, monkeypatch: pytest.MonkeyPatch
):
    required_args = contract.required.copy()
    required_args.pop(0)
    monkeypatch.setattr(
        sys,
        "argv",
        ["settlement_report_job"] + required_args,
    )
    monkeypatch.setattr(os, "environ", environment_variables)
    with pytest.raises(pydantic.ValidationError):
        CalculatorArgs()


@pytest.mark.parametrize(
    ["_", "args"],
    [
        (
            "short_grid_area_code",
            {
                "calculation-id": "12345678-9fc8-409a-a169-fbd49479d718",
                "grid-area-codes": "[0, 805]",
                "period-start-datetime": "2022-05-31T22:00:00Z",
                "period-end-datetime": "2022-06-01T22:00:00Z",
                "calculation-type": "balance_fixing",
                "created-by-user-id": "12345678-9fc8-409a-a169-fbd49479d718",
            },
        ),
        (
            "long_grid_area_code",
            {
                "calculation-id": "12345678-9fc8-409a-a169-fbd49479d718",
                "grid-area-codes": "[012341, 805]",
                "period-start-datetime": "2022-05-31T22:00:00Z",
                "period-end-datetime": "2022-06-01T22:00:00Z",
                "calculation-type": "balance_fixing",
                "created-by-user-id": "12345678-9fc8-409a-a169-fbd49479d718",
            },
        ),
    ],
)
def test_grid_area_code_validation(
    _, args, environment_variables: dict, monkeypatch: pytest.MonkeyPatch
):
    sys_args = [f"--{k}={v}" for k, v in args.items()]
    monkeypatch.setattr(sys, "argv", ["calculators"] + sys_args)
    monkeypatch.setattr(os, "environ", environment_variables)
    with pytest.raises(pydantic.ValidationError) as e:
        CalculatorArgs()
        assert "Unknown grid area code:" in str(e.value)

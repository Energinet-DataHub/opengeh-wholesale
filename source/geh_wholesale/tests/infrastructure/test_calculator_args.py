import os
import sys
from dataclasses import dataclass
from datetime import datetime
from pathlib import Path

import pydantic
import pytest

from geh_wholesale.calculation.calculator_args import CalculatorArgs
from geh_wholesale.codelists.calculation_type import CalculationType
from geh_wholesale.infrastructure.environment_variables import EnvironmentVariable
from tests import PROJECT_PATH

DEFAULT_CALCULATION_ID = "12345678-9fc8-409a-a169-fbd49479d718"
DEFAULT_ENV_VARS = {
    EnvironmentVariable.TIME_ZONE.value: "Europe/Copenhagen",
    EnvironmentVariable.QUARTERLY_RESOLUTION_TRANSITION_DATETIME.value: "2023-01-31T23:00:00Z",
}
DEFAULT_ARGS = {
    "calculation-id": "12345678-9fc8-409a-a169-fbd49479d718",
    "grid-areas": "[000, 805]",
    "period-start-datetime": "2022-05-31T22:00:00Z",
    "period-end-datetime": "2022-06-30T22:00:00Z",
    "calculation-type": "balance_fixing",
    "created-by-user-id": "12345678-9fc8-409a-a169-fbd49479d718",
}


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
            required_params.append(line.replace("{calculation-id}", DEFAULT_CALCULATION_ID))
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
    assert args.period_start_datetime == datetime.fromisoformat(args_dict["period-start-datetime"])
    assert args.period_end_datetime == datetime.fromisoformat(args_dict["period-end-datetime"])
    assert args.calculation_type.value == args_dict["calculation-type"]
    assert args.created_by_user_id == args_dict["created-by-user-id"]
    assert args.time_zone == env_args["TIME_ZONE"]
    assert args.quarterly_resolution_transition_datetime == datetime.fromisoformat(
        env_args["QUARTERLY_RESOLUTION_TRANSITION_DATETIME"]
    )


@pytest.mark.parametrize(["_", "contract"], _load_contracts().items())
def test_calculator_required_args(_, contract: Contract, monkeypatch: pytest.MonkeyPatch):
    args_dict = _args_to_dict(contract.required)
    monkeypatch.setattr(
        sys,
        "argv",
        ["calculator"] + contract.required,
    )
    monkeypatch.setattr(os, "environ", DEFAULT_ENV_VARS)
    args = CalculatorArgs()
    _assert_args(args, args_dict, DEFAULT_ENV_VARS)


@pytest.mark.parametrize(["_", "contract"], _load_contracts().items())
def test_calculator_optional_args(_, contract: Contract, monkeypatch: pytest.MonkeyPatch):
    args_dict = _args_to_dict(contract.required + contract.optional)
    monkeypatch.setattr(
        sys,
        "argv",
        ["calculator"] + contract.required + contract.optional,
    )
    monkeypatch.setattr(os, "environ", DEFAULT_ENV_VARS)
    args = CalculatorArgs()
    _assert_args(args, args_dict, DEFAULT_ENV_VARS)


@pytest.mark.parametrize(["_", "contract"], _load_contracts().items())
def test_calculator_args_missing_env(_, contract: Contract, monkeypatch: pytest.MonkeyPatch):
    environment_variables = DEFAULT_ENV_VARS.copy()
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
def test_calculator_args_required_args_missing(_, contract: Contract, monkeypatch: pytest.MonkeyPatch):
    required_args = contract.required.copy()
    required_args.pop(0)
    monkeypatch.setattr(
        sys,
        "argv",
        ["settlement_report_job"] + required_args,
    )
    monkeypatch.setattr(os, "environ", DEFAULT_ENV_VARS)
    with pytest.raises(pydantic.ValidationError):
        CalculatorArgs()


@pytest.mark.parametrize(
    ["_", "grid_areas"],
    [
        ("short_grid_area_code", "[0, 805]"),
        ("long_grid_area_code", "[012341, 805]"),
    ],
)
def test_grid_area_code_validation(_, grid_areas, monkeypatch: pytest.MonkeyPatch):
    args = DEFAULT_ARGS.copy()
    args.update({"grid-areas": grid_areas})
    sys_args = [f"--{k}={v}" for k, v in args.items()]
    monkeypatch.setattr(sys, "argv", ["calculators"] + sys_args)
    monkeypatch.setattr(os, "environ", DEFAULT_ENV_VARS)
    with pytest.raises(ValueError, match="Unknown grid area code"):
        CalculatorArgs()


@pytest.mark.parametrize(
    ["start_date", "end_date", "quarterly_resolution", "match"],
    [
        (
            "2022-05-31T22:00:00Z",
            "2022-06-01T22:00:00Z",
            "2023-01-31T22:00:00Z",
            "The quarterly resolution transition datetime must be at midnight local time.",
        ),
        (
            "2022-05-31T22:00:00Z",
            "2022-06-01T22:00:00Z",
            "2023-01-31T23:00:00Z",
            None,
        ),
        (
            "2022-05-31T22:00:00Z",
            "2022-06-02T22:00:00Z",
            "2022-06-01T22:00:00Z",
            "The calculation period must not cross the quarterly resolution transition datetime.",
        ),
    ],
)
def test_quarterly_resolution_transition_datetime_validation(
    start_date, end_date, quarterly_resolution, match, monkeypatch: pytest.MonkeyPatch
):
    args = DEFAULT_ARGS.copy()
    args.update(
        {
            "period-start-datetime": start_date,
            "period-end-datetime": end_date,
        }
    )
    env_vars = DEFAULT_ENV_VARS.copy()
    env_vars.update({"QUARTERLY_RESOLUTION_TRANSITION_DATETIME": quarterly_resolution})
    sys_args = [f"--{k}={v}" for k, v in args.items()]
    monkeypatch.setattr(sys, "argv", ["calculators"] + sys_args)
    monkeypatch.setattr(os, "environ", env_vars)
    if match:
        with pytest.raises(ValueError, match=match):
            CalculatorArgs()
    else:
        CalculatorArgs()


@pytest.mark.parametrize(
    ["calculation_type", "start_date", "end_date", "match"],
    [
        x
        for c in [
            CalculationType.WHOLESALE_FIXING,
            CalculationType.FIRST_CORRECTION_SETTLEMENT,
            CalculationType.SECOND_CORRECTION_SETTLEMENT,
            CalculationType.THIRD_CORRECTION_SETTLEMENT,
        ]
        for x in [
            (
                c.value,
                "2022-05-31T22:00:00Z",
                "2022-06-01T22:00:00Z",
                "The calculation period for wholesale calculation types must be a full month starting and ending at midnight local time",
            ),
            (
                c.value,
                "2022-05-31T22:00:00Z",
                "2022-06-30T23:00:00Z",
                "The calculation period for wholesale calculation types must be a full month starting and ending at midnight local time",
            ),
            (
                c.value,
                "2022-05-31T22:00:00Z",
                "2022-06-30T22:00:00Z",
                None,
            ),
        ]
    ],
)
def test_validate_period_for_wholesale_calculation(
    calculation_type, start_date, end_date, match, monkeypatch: pytest.MonkeyPatch
):
    args_dict = DEFAULT_ARGS.copy()
    args_dict.update(
        {
            "calculation-type": calculation_type,
            "period-start-datetime": start_date,
            "period-end-datetime": end_date,
        }
    )
    monkeypatch.setattr(sys, "argv", ["calculators"] + [f"--{k}={v}" for k, v in args_dict.items()])
    monkeypatch.setattr(os, "environ", DEFAULT_ENV_VARS)
    if match:
        with pytest.raises(ValueError, match=match):
            CalculatorArgs()
    else:
        CalculatorArgs()


@pytest.mark.parametrize(
    ["calculation_type", "match"],
    [
        (
            c,
            match,
        )
        for match in [None, "Internal calculations must be of type"]
        for c in CalculationType
    ],
)
def test_throw_exception_if_internal_calculation_and_not_aggregation_calculation_type(
    calculation_type, match, monkeypatch: pytest.MonkeyPatch
):
    args_dict = DEFAULT_ARGS.copy()
    args_dict.update(
        {
            "calculation-type": calculation_type.value,
        }
    )
    sys_args = [f"--{k}={v}" if v is not None else f"--{k}" for k, v in args_dict.items()]
    if match:
        sys_args.append("--is-internal-calculation")
    monkeypatch.setattr(sys, "argv", ["calculators"] + sys_args)
    monkeypatch.setattr(os, "environ", DEFAULT_ENV_VARS)
    if match and calculation_type != CalculationType.AGGREGATION:
        with pytest.raises(ValueError, match=match):
            CalculatorArgs()
    else:
        CalculatorArgs()

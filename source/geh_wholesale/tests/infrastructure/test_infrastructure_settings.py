import os
from itertools import combinations

import pytest

from geh_wholesale.infrastructure.environment_variables import EnvironmentVariable
from geh_wholesale.infrastructure.infrastructure_settings import InfrastructureSettings

DEFAULT_ENV_VARS = {
    EnvironmentVariable.CATALOG_NAME.value: "catalog",
    EnvironmentVariable.CALCULATION_INPUT_DATABASE_NAME.value: "calculation_input_database",
    EnvironmentVariable.DATA_STORAGE_ACCOUNT_NAME.value: "data_storage_account",
    EnvironmentVariable.TENANT_ID.value: "tenant_id",
    EnvironmentVariable.SPN_APP_ID.value: "spn_app_id",
    EnvironmentVariable.SPN_APP_SECRET.value: "spn_app_secret",
    EnvironmentVariable.MEASUREMENTS_GOLD_DATABASE_NAME.value: "measurements_gold_database",
    EnvironmentVariable.MEASUREMENTS_GOLD_CURRENT_V1_VIEW_NAME.value: "measurements_gold_current_v1_view",
}
DEFAULT_ARGS = {
    "calculation-input-folder-name": "calculation_input_folder",
    "time-series-points-table-name": "time_series_points",
    "metering-point-periods-table-name": "metering_point_periods",
    "grid-loss-metering-point-ids-table-name": "grid_loss_metering_point_ids",
}


def _get_all_combinations(args):
    out = []
    for i in range(1, len(args) + 1):
        for comb in combinations(args, i):
            out.append(comb)
    return out


def test_infrastructure_settings_with_default_env(monkeypatch):
    monkeypatch.setattr(os, "environ", DEFAULT_ENV_VARS)
    InfrastructureSettings()


@pytest.mark.parametrize("required_env_var", [(k) for k in DEFAULT_ENV_VARS.keys()])
def test_infrastructure_settings_with_missing_env(required_env_var, monkeypatch):
    env_vars = DEFAULT_ENV_VARS.copy()
    env_vars.pop(required_env_var)
    monkeypatch.setattr(os, "environ", env_vars)
    with pytest.raises(ValueError, match=required_env_var.lower()):
        InfrastructureSettings()


def test_infrastructure_settings_with_all_optional_cli_args(monkeypatch):
    sys_args = [f"--{k}={v}" for k, v in DEFAULT_ARGS.items()]
    monkeypatch.setattr(os, "environ", DEFAULT_ENV_VARS)
    monkeypatch.setattr("sys.argv", ["script"] + sys_args)
    settings = InfrastructureSettings()
    for k, v in DEFAULT_ARGS.items():
        assert settings.__dict__[k.replace("-", "_")] == v


@pytest.mark.parametrize("arg_names", _get_all_combinations(DEFAULT_ARGS.keys()))
def test_infrastructure_settings_with_some_optional_cli_args(arg_names, monkeypatch):
    sys_args = [f"--{k}={v}" for k, v in DEFAULT_ARGS.items() if k in arg_names]
    monkeypatch.setattr(os, "environ", DEFAULT_ENV_VARS)
    monkeypatch.setattr("sys.argv", ["script"] + sys_args)
    settings = InfrastructureSettings()
    for name in arg_names:
        assert settings.__dict__[name.replace("-", "_")] == DEFAULT_ARGS[name]


def test_infrastructure_settings_without_optional_cli_args(monkeypatch):
    monkeypatch.setattr(os, "environ", DEFAULT_ENV_VARS)
    monkeypatch.setattr("sys.argv", ["script"])
    settings = InfrastructureSettings()
    for k, v in DEFAULT_ARGS.items():
        assert settings.__dict__[k.replace("-", "_")] is None

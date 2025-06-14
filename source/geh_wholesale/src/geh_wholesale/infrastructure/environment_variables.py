import os
from datetime import datetime
from enum import Enum
from typing import Any

# Variables defined in the infrastructure repository (https://github.com/Energinet-DataHub/dh3-infrastructure)
from azure.identity import ClientSecretCredential


class EnvironmentVariable(Enum):
    TIME_ZONE = "TIME_ZONE"
    DATA_STORAGE_ACCOUNT_NAME = "DATA_STORAGE_ACCOUNT_NAME"
    CATALOG_NAME = "CATALOG_NAME"
    CALCULATION_INPUT_FOLDER_NAME = "CALCULATION_INPUT_FOLDER_NAME"
    CALCULATION_INPUT_DATABASE_NAME = "CALCULATION_INPUT_DATABASE_NAME"
    TENANT_ID = "TENANT_ID"
    SPN_APP_ID = "SPN_APP_ID"
    SPN_APP_SECRET = "SPN_APP_SECRET"
    QUARTERLY_RESOLUTION_TRANSITION_DATETIME = "QUARTERLY_RESOLUTION_TRANSITION_DATETIME"
    MEASUREMENTS_GOLD_DATABASE_NAME = "MEASUREMENTS_GOLD_DATABASE_NAME"
    AZURE_APP_CONFIGURATION_ENDPOINT = "AZURE_APP_CONFIGURATION_ENDPOINT"


def get_storage_account_credential() -> ClientSecretCredential:
    required_env_variables = [
        EnvironmentVariable.TENANT_ID,
        EnvironmentVariable.SPN_APP_ID,
        EnvironmentVariable.SPN_APP_SECRET,
        EnvironmentVariable.DATA_STORAGE_ACCOUNT_NAME,
    ]
    env_vars = get_env_variables_or_throw(required_env_variables)

    credential = ClientSecretCredential(
        env_vars[EnvironmentVariable.TENANT_ID],
        env_vars[EnvironmentVariable.SPN_APP_ID],
        env_vars[EnvironmentVariable.SPN_APP_SECRET],
    )

    return credential


def get_storage_account_name() -> str:
    return get_env_variable_or_throw(EnvironmentVariable.DATA_STORAGE_ACCOUNT_NAME)


def get_time_zone() -> str:
    return get_env_variable_or_throw(EnvironmentVariable.TIME_ZONE)


def get_quarterly_resolution_transition_datetime() -> datetime:
    quarterly_resolution_transition_datetime = get_env_variable_or_throw(
        EnvironmentVariable.QUARTERLY_RESOLUTION_TRANSITION_DATETIME
    )
    return datetime.strptime(quarterly_resolution_transition_datetime, "%Y-%m-%dT%H:%M:%SZ")


def get_catalog_name() -> str:
    return get_env_variable_or_throw(EnvironmentVariable.CATALOG_NAME)


def get_calculation_input_folder_name() -> str:
    return get_env_variable_or_throw(EnvironmentVariable.CALCULATION_INPUT_FOLDER_NAME)


def get_calculation_input_database_name() -> str:
    return get_env_variable_or_throw(EnvironmentVariable.CALCULATION_INPUT_DATABASE_NAME)


def get_env_variables_or_throw(environment_variable: list[EnvironmentVariable]) -> dict:
    env_variables = dict()
    for env_var in environment_variable:
        env_variables[env_var] = get_env_variable_or_throw(env_var)

    return env_variables


def get_env_variable_or_throw(variable: EnvironmentVariable) -> Any:
    env_variable = os.getenv(variable.name)
    if env_variable is None:
        raise ValueError(f"Environment variable not found: {variable.name}")

    return env_variable

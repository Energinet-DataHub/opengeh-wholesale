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

from dataclasses import dataclass, field
from pydantic_settings import BaseSettings, CliSettingsSource, PydanticBaseSettingsSource
from typing import Tuple, Type, Optional
from azure.identity import ClientSecretCredential


# @dataclass
# class InfrastructureSettings:
#     catalog_name: str
#     calculation_input_database_name: str
#     data_storage_account_name: str
#     # Prevent the credentials from being printed or logged (using e.g. print() or repr())
#     data_storage_account_credentials: ClientSecretCredential = field(repr=False)
#     wholesale_container_path: str
#     calculation_input_path: str
#     time_series_points_table_name: str | None
#     metering_point_periods_table_name: str | None
#     grid_loss_metering_point_ids_table_name: str | None

class InfrastructureSettings(BaseSettings):
    """
    InfrastructureSettings class uses Pydantic BaseSettings to configure and validate parameters.
    Parameters can come from both runtime (CLI) or from environment variables.
    The priority is CLI parameters first and then environment variables.
    """
    catalog_name: Optional[str] = None
    calculation_input_database_name: Optional[str] = None
    data_storage_account_name: Optional[str] = None
    # Prevent the credentials from being printed or logged (using e.g., print() or repr())
    data_storage_account_credentials: Optional[ClientSecretCredential] = field(default=None, repr=False)
    wholesale_container_path: Optional[str] = None
    calculation_input_path: Optional[str] = None
    time_series_points_table_name: Optional[str] = None
    metering_point_periods_table_name: Optional[str] = None
    grid_loss_metering_point_ids_table_name: Optional[str] = None

    @classmethod
    def settings_customise_sources(
        cls,
        settings_cls: Type[BaseSettings],
        init_settings: PydanticBaseSettingsSource,
        env_settings: PydanticBaseSettingsSource,
        dotenv_settings: PydanticBaseSettingsSource,
        file_secret_settings: PydanticBaseSettingsSource,
    ) -> Tuple[PydanticBaseSettingsSource, ...]:
        return CliSettingsSource(settings_cls, cli_parse_args=True, cli_ignore_unknown_args=True), env_settings

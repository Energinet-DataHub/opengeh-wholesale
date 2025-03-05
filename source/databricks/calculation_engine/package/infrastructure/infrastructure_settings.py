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

from azure.identity import ClientSecretCredential
from pydantic import AliasChoices, Field

from package.infrastructure import paths
from geh_common.application.settings import ApplicationSettings

from typing import Any


class InfrastructureSettings(ApplicationSettings, cli_kebab_case=False):  # type: ignore
    """
    InfrastructureSettings class uses Pydantic BaseSettings to configure and validate parameters.
    Parameters can come from both runtime (CLI) or from environment variables.
    The priority is CLI parameters first and then environment variables.
    """

    catalog_name: str = Field(init=False)  # From ENVIRONMENT
    calculation_input_database_name: str = Field(init=False)  # From ENVIRONMENT
    data_storage_account_name: str = Field(init=False)  # From ENVIRONMENT
    # Prevent the credentials from being printed or logged (using e.g., print() or repr())
    tenant_id: str = Field(repr=False, init=False)  # Ud? #From ENVIRONMENT
    spn_app_id: str = Field(repr=False, init=False)  # Ud? #From ENVIRONMENT
    spn_app_secret: str = Field(repr=False, init=False)  # Ud? #From ENVIRONMENT

    calculation_input_folder_name: str | None = Field(
        init=False,
        default=None,
        validation_alias=AliasChoices(
            "calculation_input_folder_name", "calculation-input-folder-name"
        ),
    )  # Ud? From CLI/ENVIRONMENT

    time_series_points_table_name: str | None = Field(
        init=False,
        default=None,
        validation_alias=AliasChoices(
            "time_series_points_table_name", "time-series-points-table-name"
        ),
    )  # From CLI
    metering_point_periods_table_name: str | None = Field(
        init=False,
        default=None,
        validation_alias=AliasChoices(
            "metering_point_periods_table_name", "metering-point-periods-table-name"
        ),
    )  # From CLI
    grid_loss_metering_point_ids_table_name: str | None = Field(
        init=False,
        default=None,
        validation_alias=AliasChoices(
            "grid_loss_metering_points_table_name",
            "grid-loss-metering-points-table-name",
            "grid_loss_metering_point_ids_table_name",
        ),
    )  # From CLI

    data_storage_account_credentials: ClientSecretCredential | None = Field(
        default=None
    )
    wholesale_container_path: str | None = Field(default=None)
    calculation_input_path: str | None = Field(default=None)

    def model_post_init(self, __context: Any) -> None:
        self.data_storage_account_credentials = (
            self.data_storage_account_credentials
            or ClientSecretCredential(
                tenant_id=self.tenant_id,
                client_id=self.spn_app_id,
                client_secret=self.spn_app_secret,
            )
        )
        self.wholesale_container_path = (
            self.wholesale_container_path
            or paths.get_container_root_path(str(self.data_storage_account_name))
        )
        self.calculation_input_path = (
            self.calculation_input_path
            or paths.get_calculation_input_path(
                str(self.data_storage_account_name),
                str(self.calculation_input_folder_name),
            )
        )

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

from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict


class InfrastructureSettings(BaseSettings):  # type: ignore
    """
    InfrastructureSettings class uses Pydantic BaseSettings to configure and validate parameters.
    Parameters can come from both runtime (CLI) or from environment variables.
    The priority is CLI parameters first and then environment variables.
    """

    model_config = SettingsConfigDict(
        cli_prog_name="infrastructure",
        cli_parse_args=True,
        cli_kebab_case=True,
        cli_ignore_unknown_args=True,
        cli_implicit_flags=True,
    )

    catalog_name: str = Field(init=False)
    calculation_input_database_name: str = Field(init=False)
    data_storage_account_name: str = Field(init=False)

    # (repr=False) prevents the field from being printed in the repr of the model
    tenant_id: str = Field(init=False, repr=False)
    spn_app_id: str = Field(init=False, repr=False)
    spn_app_secret: str = Field(init=False, repr=False)

    calculation_input_folder_name: str | None = Field(init=False, default=None)
    time_series_points_table_name: str | None = Field(init=False, default=None)
    metering_point_periods_table_name: str | None = Field(init=False, default=None)
    grid_loss_metering_point_ids_table_name: str | None = Field(
        init=False, default=None
    )

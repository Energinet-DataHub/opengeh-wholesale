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

from dataclasses import dataclass
from datetime import datetime, timezone
from pydantic_settings import BaseSettings, CliSettingsSource, PydanticBaseSettingsSource
from pydantic.dataclasses import Field
from typing import Tuple, Type, Optional
from package.codelists.calculation_type import (
    CalculationType,
)
import os


# @dataclass
# class CalculatorArgs:
#     calculation_id: str
#     calculation_grid_areas: list[str]
#     calculation_period_start_datetime: datetime
#     calculation_period_end_datetime: datetime
#     calculation_type: CalculationType
#     calculation_execution_time_start: datetime
#     created_by_user_id: str
#     time_zone: str
#     quarterly_resolution_transition_datetime: datetime
#     is_internal_calculation: bool


class CalculatorArgs(BaseSettings):
    """
    CalculatorArgs class uses Pydantic BaseSettings to configure and validate parameters.
    Parameters can come from both runtime (CLI) or from environment variables.
    The priority is CLI parameters first and then environment variables.
    """
    calculation_id: str = Field(..., alias="calculation-id") #From CLI
    calculation_grid_areas: list[str] = Field(..., alias="grid-areas") #From CLI
    calculation_period_start_datetime: datetime = Field(..., alias="period-start-datetime") #From CLI
    calculation_period_end_datetime: datetime = Field(..., alias="period-end-datetime") #From CLI
    calculation_type: CalculationType = Field(..., alias="calculation-type") #From CLI
    calculation_execution_time_start: datetime = datetime.now(timezone.utc)
    created_by_user_id: str = Field(..., alias="created-by-user-id") #From CLI
    time_zone: str #From ENVIRONMENT
    quarterly_resolution_transition_datetime: datetime #From ENVIRONMENT
    is_internal_calculation: Optional[bool] = Field(default=False, alias="is-internal-calculation") #From CLI

    @classmethod
    def settings_customise_sources(
        cls,
        settings_cls: Type[BaseSettings],
        init_settings: PydanticBaseSettingsSource,
        env_settings: PydanticBaseSettingsSource,
        dotenv_settings: PydanticBaseSettingsSource,
        file_secret_settings: PydanticBaseSettingsSource,
    ) -> Tuple[PydanticBaseSettingsSource, ...]:
        return CliSettingsSource(settings_cls, cli_parse_args=True, cli_ignore_unknown_args=True), env_settings, init_settings



# os.environ['TIME_ZONE'] = 'tzutc'
# os.environ['QUARTERLY_RESOLUTION_TRANSITION_DATETIME'] = '1989-01-01'
#
# _settings = CalculatorArgs()
# #
# for s in _settings:
#     print(s)

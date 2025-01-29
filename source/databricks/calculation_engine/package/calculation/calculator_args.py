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
from datetime import datetime
from pydantic_settings import BaseSettings, CliSettingsSource, PydanticBaseSettingsSource
from typing import Tuple, Type, Optional
from package.codelists.calculation_type import (
    CalculationType,
)


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

import os

os.environ["TIME_ZONE"] = "test_time_zone"

class CalculatorArgs(BaseSettings):
    """
    CalculatorArgs class uses Pydantic BaseSettings to configure and validate parameters.
    Parameters can come from both runtime (CLI) or from environment variables.
    The priority is CLI parameters first and then environment variables.
    """
    calculation_id: Optional[str] = None
    calculation_grid_areas: Optional[list[str]] = None
    calculation_period_start_datetime: Optional[datetime] = None
    calculation_period_end_datetime: Optional[datetime] = None
    calculation_type: Optional[CalculationType] = None
    calculation_execution_time_start: Optional[datetime] = None
    created_by_user_id: Optional[str] = None
    time_zone: Optional[str] = None
    quarterly_resolution_transition_datetime: Optional[datetime] = None
    is_internal_calculation: Optional[bool] = None

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



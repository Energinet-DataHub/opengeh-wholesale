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


from datetime import datetime, timezone
from pydantic_settings import BaseSettings
from pydantic import AliasChoices, Field

from package.codelists.calculation_type import (
    CalculationType,
)


# -------- Should be moved to Packages Repo
class ApplicationSettings(
    BaseSettings,
    cli_parse_args=True,
    cli_kebab_case=True,
    cli_ignore_unknown_args=True,
    cli_implicit_flags=True,
):
    """
    Base class for application settings.
    Supports:
    - CLI parsing with arguments using kebab-case.
    - Environment variables using SNAKE_UPPER_CASE.
    - Ignoring unknown CLI arguments. This behavior can be overridden by setting `cli_ignore_unknown_args=False`
      in the class definition of the derived settings class. Example:
      `class Settings(ApplicationSettings, cli_ignore_unknown_args=False):`
    """

    pass


class CalculatorArgs(ApplicationSettings):
    """
    CalculatorArgs class uses Pydantic BaseSettings to configure and validate parameters.
    Parameters can come from both runtime (CLI) or from environment variables.
    The priority is CLI parameters first and then environment variables.
    """

    calculation_id: str  # From CLI
    calculation_grid_areas: list[str] = Field(
        ...,
        validation_alias=AliasChoices(
            "grid_areas", "grid-areas", "calculation_grid_areas"
        ),
    )  # From CLI
    calculation_period_start_datetime: datetime = Field(
        ...,
        validation_alias=AliasChoices(
            "period_start_datetime",
            "period-start-datetime",
            "calculation_period_start_datetime",
        ),
    )  # From CLI
    calculation_period_end_datetime: datetime = Field(
        ...,
        validation_alias=AliasChoices(
            "period_end_datetime",
            "period-end-datetime",
            "calculation_period_end_datetime",
        ),
    )  # From CLI
    calculation_type: CalculationType  # From CLI
    calculation_execution_time_start: datetime = Field(
        default=datetime.now(timezone.utc)
    )
    created_by_user_id: str  # From CLI
    time_zone: str  # From ENVIRONMENT
    quarterly_resolution_transition_datetime: datetime  # From ENVIRONMENT
    is_internal_calculation: bool = Field(default=False)

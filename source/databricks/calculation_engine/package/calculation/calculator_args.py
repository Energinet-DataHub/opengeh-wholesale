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
from package.codelists.calculation_type import (
    CalculationType,
    is_wholesale_calculation_type,
)
from typing import Any, Annotated
from pydantic import AliasChoices, Field, field_validator
from pydantic_settings import NoDecode
from geh_common.application.settings import ApplicationSettings

from package.common.datetime_utils import (
    is_exactly_one_calendar_month,
    is_midnight_in_time_zone,
)
import re


class CalculatorArgs(ApplicationSettings):
    """
    CalculatorArgs class uses Pydantic BaseSettings to configure and validate parameters.
    Parameters can come from both runtime (CLI) or from environment variables.
    The priority is CLI parameters first and then environment variables.
    """

    calculation_id: str  # From CLI
    calculation_grid_areas: Annotated[list[str], NoDecode] = Field(
        init=False,
        validation_alias=AliasChoices(
            "calculation_grid_areas", "grid_areas", "grid-areas"
        ),
    )
    # From CLI

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

    @field_validator("calculation_grid_areas", mode="before")
    @classmethod
    def _validate_myvar(cls, value: Any) -> list[str]:
        return re.findall(r"\d+", value)


class CalculatorArgsValidation:
    def __init__(self, args: CalculatorArgs):
        self.args = args
        self.validate()

    def _validate_quarterly_resolution_transition_datetime(self) -> None:
        if (
            is_midnight_in_time_zone(
                self.args.quarterly_resolution_transition_datetime, self.args.time_zone
            )
            is False
        ):
            raise Exception(
                f"The quarterly resolution transition datetime must be at midnight local time ({self.args.time_zone})."
            )
        if (
            self.args.calculation_period_start_datetime
            < self.args.quarterly_resolution_transition_datetime
            < self.args.calculation_period_end_datetime
        ):
            raise Exception(
                "The calculation period must not cross the quarterly resolution transition datetime."
            )

    def _validate_period_for_wholesale_calculation(self) -> None:
        is_valid_period = is_exactly_one_calendar_month(
            self.args.calculation_period_start_datetime,
            self.args.calculation_period_end_datetime,
            self.args.time_zone,
        )
        if is_wholesale_calculation_type(self.args.calculation_type):
            if not is_valid_period:
                raise Exception(
                    f"The calculation period for wholesale calculation types must be a full month starting and ending at midnight local time ({self.args.time_zone}))."
                )

    def _throw_exception_if_internal_calculation_and_not_aggregation_calculation_type(
        self,
    ) -> None:
        if (
            self.args.is_internal_calculation
            and self.args.calculation_type != CalculationType.AGGREGATION
        ):
            raise Exception("Internal calculations must be of type AGGREGATION. ")

    def validate(self) -> None:
        """Runs all validation methods."""
        self._validate_quarterly_resolution_transition_datetime()
        self._validate_period_for_wholesale_calculation()
        self._throw_exception_if_internal_calculation_and_not_aggregation_calculation_type()

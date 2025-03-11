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
from pydantic import AliasChoices, Field, field_validator, model_validator
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

    calculation_id: str = Field(init=False)  # From CLI
    calculation_grid_areas: Annotated[list[str], NoDecode] = Field(
        init=False,
        validation_alias=AliasChoices(
            "calculation_grid_areas", "grid_areas", "grid-areas"
        ),
    )
    # From CLI

    calculation_period_start_datetime: datetime = Field(
        init=False,
        validation_alias=AliasChoices(
            "period_start_datetime",
            "period-start-datetime",
            "calculation_period_start_datetime",
        ),
    )  # From CLI
    calculation_period_end_datetime: datetime = Field(
        init=False,
        validation_alias=AliasChoices(
            "period_end_datetime",
            "period-end-datetime",
            "calculation_period_end_datetime",
        ),
    )  # From CLI
    calculation_type: CalculationType = Field(init=False)  # From CLI
    calculation_execution_time_start: datetime = Field(
        default=datetime.now(timezone.utc)
    )
    created_by_user_id: str = Field(init=False)  # From CLI
    time_zone: str = Field(init=False)  # From ENVIRONMENT
    quarterly_resolution_transition_datetime: datetime = Field(
        init=False
    )  # From ENVIRONMENT
    is_internal_calculation: bool = Field(default=False, init=False)

    @field_validator("calculation_grid_areas", mode="before")
    @classmethod
    def _validate_grid_areas(cls, value: Any) -> list[str]:
        if isinstance(value, list):
            return [str(item) for item in value]
        elif isinstance(value, str):
            return re.findall(r"\d+", value)
        else:
            raise ValueError(
                f"The grid areas must be a list of strings or a string, not {type(value)}"
            )

    @model_validator(mode="after")
    def _validate_quarterly_resolution_transition_datetime(self) -> "CalculatorArgs":
        is_midnight, local_time = is_midnight_in_time_zone(
            self.quarterly_resolution_transition_datetime, self.time_zone
        )
        if not is_midnight:
            raise Exception(
                f"The quarterly resolution transition datetime must be at midnight local time. {self.quarterly_resolution_transition_datetime} coverted to '{self.time_zone}' is {local_time}",
            )
        if (
            self.calculation_period_start_datetime
            < self.quarterly_resolution_transition_datetime
            < self.calculation_period_end_datetime
        ):
            raise Exception(
                "The calculation period must not cross the quarterly resolution transition datetime."
            )
        return self

    @model_validator(mode="after")
    def _validate_period_for_wholesale_calculation(self) -> "CalculatorArgs":
        is_valid_period = is_exactly_one_calendar_month(
            self.calculation_period_start_datetime,
            self.calculation_period_end_datetime,
            self.time_zone,
        )
        if is_wholesale_calculation_type(self.calculation_type):
            if not is_valid_period:
                raise Exception(
                    f"The calculation period for wholesale calculation types must be a full month starting and ending at midnight local time ({self.time_zone}))."
                )
        return self

    @model_validator(mode="after")
    def _throw_exception_if_internal_calculation_and_not_aggregation_calculation_type(
        self,
    ) -> "CalculatorArgs":
        if (
            self.is_internal_calculation
            and self.calculation_type != CalculationType.AGGREGATION
        ):
            raise Exception("Internal calculations must be of type AGGREGATION. ")
        return self

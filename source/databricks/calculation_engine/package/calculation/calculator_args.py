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
from pydantic import Field, field_validator, model_validator
from pydantic_settings import BaseSettings, NoDecode, SettingsConfigDict

from package.common.datetime_utils import (
    is_exactly_one_calendar_month,
    is_midnight_in_time_zone,
)
import re


class CalculatorArgs(BaseSettings):
    """
    CalculatorArgs class uses Pydantic BaseSettings to configure and validate parameters.
    Parameters can come from both runtime (CLI) or from environment variables.
    The priority is CLI parameters first and then environment variables.
    """

    model_config = SettingsConfigDict(
        cli_prog_name="calculator",
        cli_parse_args=True,
        cli_kebab_case=True,
        cli_ignore_unknown_args=True,
        cli_implicit_flags=False,
    )

    # Required CLI parameters
    calculation_id: str = Field(init=False)
    grid_areas: Annotated[list[str], NoDecode] = Field(init=False)
    period_start_datetime: datetime = Field(init=False)
    period_end_datetime: datetime = Field(init=False)
    calculation_type: CalculationType = Field(init=False)
    created_by_user_id: str = Field(init=False)

    # Optional CLI parameters
    is_internal_calculation: bool = Field(init=False, default=False)

    # Environment variables
    time_zone: str = Field(init=False)
    quarterly_resolution_transition_datetime: datetime = Field(init=False)

    # Default values
    calculation_execution_time_start: datetime = Field(
        init=False, default=datetime.now(timezone.utc)
    )

    @field_validator(
        "period_start_datetime",
        "period_end_datetime",
        "quarterly_resolution_transition_datetime",
        mode="after",
    )
    @classmethod
    def _to_utc_datetime(cls, value: datetime) -> datetime:
        return value.replace(tzinfo=timezone.utc)

    @field_validator("grid_areas", mode="before")
    @classmethod
    def _convert_grid_area_codes(cls, value: Any) -> list[str]:
        if isinstance(value, list):
            return [str(item) for item in value]
        elif isinstance(value, str):
            return re.findall(r"\d+", value)
        else:
            raise ValueError(
                f"The grid areas must be a list of strings or a string, not {type(value)}"
            )

    @field_validator("grid_areas", mode="after")
    @classmethod
    def validate_grid_area_codes(cls, v: list[str] | None) -> list[str] | None:
        if v is None:
            return v
        for code in v:
            assert isinstance(code, str), (
                f"Grid area codes must be strings, not {type(code)}"
            )
            if len(code) != 3 or not code.isdigit():
                raise ValueError(
                    f"Unknown grid area code: '{code}'. Grid area codes must consist of 3 digits (000-999)."
                )
        return v

    @model_validator(mode="after")
    def _validate_quarterly_resolution_transition_datetime(self) -> "CalculatorArgs":
        is_midnight, local_time = is_midnight_in_time_zone(
            self.quarterly_resolution_transition_datetime, self.time_zone
        )
        if not is_midnight:
            raise ValueError(
                f"The quarterly resolution transition datetime must be at midnight local time. {self.quarterly_resolution_transition_datetime} coverted to '{self.time_zone}' is {local_time}",
            )
        if (
            self.period_start_datetime
            < self.quarterly_resolution_transition_datetime
            < self.period_end_datetime
        ):
            raise ValueError(
                "The calculation period must not cross the quarterly resolution transition datetime."
            )
        return self

    @model_validator(mode="after")
    def _validate_period_for_wholesale_calculation(self) -> "CalculatorArgs":
        is_valid_period = is_exactly_one_calendar_month(
            self.period_start_datetime,
            self.period_end_datetime,
            self.time_zone,
        )
        if is_wholesale_calculation_type(self.calculation_type):
            if not is_valid_period:
                raise ValueError(
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
            raise ValueError(
                f"Internal calculations must be of type {CalculationType.AGGREGATION.value}. Got: {self.calculation_type}"
            )
        return self

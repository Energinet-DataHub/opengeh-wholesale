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
import sys
from datetime import datetime, timezone
from zoneinfo import ZoneInfo

import pytest
from pyspark.sql import SparkSession

import tests.calculation.preparation.transformations.prepared_metering_point_time_series_factory as factory
from geh_wholesale.calculation.calculator_args import CalculatorArgs
from geh_wholesale.calculation.energy.resolution_transition_factory import (
    get_energy_result_resolution,
    get_energy_result_resolution_adjusted_metering_point_time_series,
)
from geh_wholesale.codelists import CalculationType, MeteringPointResolution


class TestGetEnergyResultResolution:
    @pytest.mark.parametrize(
        "transition_datetime, end_datetime, expected",
        [
            (
                datetime(2023, 4, 30, 23),
                datetime(2023, 3, 31, 22),
                MeteringPointResolution.HOUR,
            ),
            (
                datetime(2023, 4, 30, 23),
                datetime(2023, 5, 31, 23),
                MeteringPointResolution.QUARTER,
            ),
            (
                datetime(2023, 4, 30, 23),
                datetime(2023, 4, 30, 23),
                MeteringPointResolution.HOUR,
            ),
        ],
    )
    def test_returns_hour_when_period_end_is_before_or_equal_to_transition_datetime_and_quarter_if_after(
        self,
        transition_datetime: datetime,
        end_datetime: datetime,
        expected: MeteringPointResolution,
    ) -> None:
        actual = get_energy_result_resolution(transition_datetime, end_datetime)
        assert actual == expected


class TestEnergyResultResolutionAdjustedMeteringPointTimeSeries:
    @pytest.mark.parametrize(
        "transition_datetime, start_datetime, end_datetime, expected_rows",
        [
            (
                datetime(2023, 4, 30, 22, tzinfo=timezone.utc),
                datetime(2023, 2, 28, 23, tzinfo=timezone.utc),
                datetime(2023, 3, 31, 22, tzinfo=timezone.utc),
                2,
            ),
            (
                datetime(2023, 4, 30, 22, tzinfo=timezone.utc),
                datetime(2023, 4, 30, 22, tzinfo=timezone.utc),
                datetime(2023, 5, 31, 22, tzinfo=timezone.utc),
                8,
            ),
            (
                datetime(2023, 4, 30, 22, tzinfo=timezone.utc),
                datetime(2023, 3, 31, 22, tzinfo=timezone.utc),
                datetime(2023, 4, 30, 22, tzinfo=timezone.utc),
                2,
            ),
        ],
    )
    def test_transforms_to_hour_when_period_end_is_before_or_equal_to_transition_datetime_and_quarter_if_after(
        self,
        transition_datetime: datetime,
        start_datetime: datetime,
        end_datetime: datetime,
        expected_rows: int,
        spark: SparkSession,
        monkeypatch: pytest.MonkeyPatch,
    ) -> None:
        # Arrange
        monkeypatch.setattr(
            sys,
            "argv",
            [
                CalculatorArgs.model_config.get("cli_prog_name", "calculator"),
                "--calculation-id=test_id",
                "--grid-areas=[123]",
                f"--calculation-type={CalculationType.BALANCE_FIXING.value}",
                f"--calculation-execution-time-start={datetime.now(timezone.utc)}",
                "--created-by-user-id=test_user",
                f"--period-start-datetime={start_datetime}",
                f"--period-end-datetime={end_datetime}",
                f"--calculation-period-end-datetime={end_datetime}",
                f"--quarterly-resolution-transition-datetime={transition_datetime}",
            ],
        )
        monkeypatch.setenv("TIME_ZONE", "Europe/Copenhagen")
        args = CalculatorArgs()
        rows = [
            factory.create_row(
                resolution=MeteringPointResolution.HOUR,
                observation_time=datetime(2023, 4, 1, 0, 0, tzinfo=ZoneInfo("Europe/Copenhagen")),
            ),
            factory.create_row(
                resolution=MeteringPointResolution.QUARTER,
                observation_time=datetime(2023, 4, 1, 0, 0, tzinfo=ZoneInfo("Europe/Copenhagen")),
            ),
            factory.create_row(
                resolution=MeteringPointResolution.QUARTER,
                observation_time=datetime(2023, 4, 1, 0, 15, tzinfo=ZoneInfo("Europe/Copenhagen")),
            ),
            factory.create_row(
                resolution=MeteringPointResolution.QUARTER,
                observation_time=datetime(2023, 4, 1, 0, 30, tzinfo=ZoneInfo("Europe/Copenhagen")),
            ),
            factory.create_row(
                resolution=MeteringPointResolution.QUARTER,
                observation_time=datetime(2023, 4, 1, 0, 45, tzinfo=ZoneInfo("Europe/Copenhagen")),
            ),
        ]
        prepared_metering_point_time_series = factory.create(spark, rows)

        # Act
        actual = get_energy_result_resolution_adjusted_metering_point_time_series(
            args, prepared_metering_point_time_series
        )

        # Assert
        assert actual.df.count() == expected_rows

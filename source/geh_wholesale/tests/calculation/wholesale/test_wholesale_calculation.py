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
import uuid

import pytest
from pyspark.sql import SparkSession

import tests.calculation.wholesale.factories.prepared_fees_factory as fees_factory
import tests.calculation.wholesale.factories.prepared_subscriptions_factory as subscriptions_factory
import tests.calculation.wholesale.factories.prepared_tariffs_factory as tariffs_factory
from geh_wholesale.calculation.calculator_args import CalculatorArgs
from geh_wholesale.calculation.preparation.data_structures.prepared_charges import (
    PreparedChargesContainer,
)
from geh_wholesale.calculation.wholesale import execute
from geh_wholesale.codelists import ChargeResolution
from geh_wholesale.codelists.calculation_type import CalculationType


def test__execute__when_tariff_schema_is_valid__does_not_raise(
    spark: SparkSession, monkeypatch: pytest.MonkeyPatch
) -> None:
    monkeypatch.setattr(
        sys,
        "argv",
        [
            CalculatorArgs.model_config.get("cli_prog_name", "calculator"),
            "--calculation-id=foo",
            f"--calculation-type={CalculationType.WHOLESALE_FIXING.value}",
            "--grid-areas=[805,806]",
            "--period-start-datetime=2022-06-30T22:00:00Z",
            "--period-end-datetime=2022-07-31T22:00:00Z",
            "--calculation-execution-time-start=2022-08-01T22:00:00Z",
            f"--created-by-user-id={uuid.uuid4()}",
        ],
    )
    monkeypatch.setenv("TIME_ZONE", "Europe/Copenhagen")
    monkeypatch.setenv("QUARTERLY_RESOLUTION_TRANSITION_DATETIME", "2023-01-31T23:00:00Z")
    args = CalculatorArgs()
    # Arrange
    tariffs_hourly_df = tariffs_factory.create(spark, data=[tariffs_factory.create_row()])
    tariffs_daily_df = tariffs_factory.create(
        spark,
        data=[tariffs_factory.create_row(resolution=ChargeResolution.DAY)],
    )
    prepared_subscriptions = subscriptions_factory.create(spark)
    prepared_fees = fees_factory.create(spark)

    prepared_charges = PreparedChargesContainer(
        fees=prepared_fees,
        subscriptions=prepared_subscriptions,
        hourly_tariffs=tariffs_hourly_df,
        daily_tariffs=tariffs_daily_df,
    )

    # Act
    execute(args, prepared_charges)

    # Assert
    # If execute raises an exception, the test fails automatically

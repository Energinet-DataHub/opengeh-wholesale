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
import uuid
from datetime import datetime
from unittest.mock import patch

import pytest
from pyspark.sql.session import SparkSession

from package.codelists import CalculationType
from settlement_report_job.domain.energy_results_factory import create_energy_results
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs


@pytest.fixture(scope="session")
def settlement_report_args() -> SettlementReportArgs:
    return SettlementReportArgs(
        report_id=str(uuid.uuid4()),
        period_start=datetime(2022, 12, 31, 22, 0, 0),
        period_end=datetime(2023, 1, 31, 22, 0, 0),
        calculation_type=CalculationType.WHOLESALE_FIXING,
        split_report_by_grid_area=True,
        prevent_large_text_files=False,
        time_zone="Europe/Copenhagen",
        catalog_name="catalog_name",
        market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        calculation_id_by_grid_area={
            "804": uuid.UUID("aa3b3b3b-3b3b-3b3b-3b3b-3b3b3b3b3b3b"),
            "805": uuid.UUID("bb3b3b3b-3b3b-3b3b-3b3b-3b3b3b3b3b3b"),
        },
        energy_supplier_id="energy_supplier_id",
    )


def test__create_energy_results__returns_expected(
    spark: SparkSession,
    settlement_report_args: SettlementReportArgs,
    job_environment_variables: dict[str, str],
) -> None:
    # Arrange
    energy_report_input = spark.read.csv("energy_v1.csv", header=True, sep=";")

    # Act
    with patch("pyspark.sql.SparkSession.read") as mock_read:
        mock_read.table.return_value = energy_report_input
        with patch.dict("os.environ", job_environment_variables):
            actual = create_energy_results(spark, settlement_report_args)

    # Assert
    actual.show()

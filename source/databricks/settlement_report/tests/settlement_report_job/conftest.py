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

import pytest

from settlement_report_job.calculation_type import CalculationType
from settlement_report_job.settlement_report_args import SettlementReportArgs


@pytest.fixture(scope="session")
def any_settlement_report_args() -> SettlementReportArgs:
    return SettlementReportArgs(
        report_id=str(uuid.uuid4()),
        period_start=datetime(2018, 3, 31, 22, 0, 0),
        period_end=datetime(2018, 4, 30, 22, 0, 0),
        calculation_type=CalculationType.WHOLESALE_FIXING,
        split_report_by_grid_area=True,
        prevent_large_text_files=False,
        time_zone="Europe/Copenhagen",
        catalog_name="catalog_name",
    )

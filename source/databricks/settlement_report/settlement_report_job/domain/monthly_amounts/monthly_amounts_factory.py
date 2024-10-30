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

from pyspark.sql import DataFrame

import settlement_report_job.domain.report_naming_convention as market_naming
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs

from settlement_report_job.domain.monthly_amounts.read_and_filter import (
    read_and_filter_from_view,
)
from settlement_report_job.domain.monthly_amounts.prepare_for_csv import (
    prepare_for_csv,
)
from settlement_report_job.domain.factory_utils import (
    _should_have_one_file_per_grid_area,
)


def create_monthly_amounts(
    args: SettlementReportArgs,
    repository: WholesaleRepository,
) -> DataFrame:
    monthly_amounts = read_and_filter_from_view(args, repository)

    return prepare_for_csv(
        monthly_amounts,
        _should_have_one_file_per_grid_area(args=args),
    )

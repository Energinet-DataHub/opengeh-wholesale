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
from settlement_report_job.infrastructure.repository import WholesaleRepository
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from telemetry_logging import use_span

from settlement_report_job.domain.utils.settlement_report_args_utils import (
    should_have_result_file_per_grid_area,
)
from settlement_report_job.domain.wholesale_results.read_and_filter import (
    read_and_filter_from_view,
)
from settlement_report_job.domain.wholesale_results.prepare_for_csv import (
    prepare_for_csv,
)


@use_span()
def create_wholesale_results(
    args: SettlementReportArgs,
    repository: WholesaleRepository,
) -> DataFrame:
    wholesale = read_and_filter_from_view(
        args.energy_supplier_ids,
        args.calculation_id_by_grid_area,
        args.period_start,
        args.period_end,
        args.requesting_actor_market_role,
        args.requesting_actor_id,
        repository,
    )

    return prepare_for_csv(
        wholesale,
        should_have_result_file_per_grid_area(args),
    )

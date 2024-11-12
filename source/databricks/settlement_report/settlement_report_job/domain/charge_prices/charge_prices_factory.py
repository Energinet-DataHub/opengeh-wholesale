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

from settlement_report_job.domain.charge_prices.read_and_filter import read_and_filter
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.entry_points.job_args.settlement_report_args import SettlementReportArgs


def create_charge_prices(
    args: SettlementReportArgs,
    repository: WholesaleRepository,
) -> DataFrame:
    charge_prices = read_and_filter(
        args.period_start,
        args.period_end,
        args.calculation_id_by_grid_area,
        args.energy_supplier_ids,
        args.requesting_actor_market_role,
        args.requesting_actor_id,
        repository,
    )

    return charge_prices

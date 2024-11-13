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
from collections.abc import Callable

from pyspark.sql import DataFrame, functions as F

from telemetry_logging import Logger, use_span
from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.infrastructure.repository import WholesaleRepository
from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from settlement_report_job.entry_points.job_args.calculation_type import CalculationType
from settlement_report_job.domain.utils.factory_filters import (
    filter_by_energy_supplier_ids,
    filter_by_grid_area_codes,
    filter_by_calculation_id_by_grid_area,
    read_and_filter_by_latest_calculations,
)

log = Logger(__name__)


def _get_view_read_function(
    requesting_actor_market_role: MarketRole,
    repository: WholesaleRepository,
) -> Callable[[], DataFrame]:
    if requesting_actor_market_role == MarketRole.GRID_ACCESS_PROVIDER:
        return repository.read_energy
    else:
        return repository.read_energy_per_es


@use_span()
def read_and_filter_from_view(
    args: SettlementReportArgs, repository: WholesaleRepository
) -> DataFrame:
    read_from_repository_func = _get_view_read_function(
        args.requesting_actor_market_role, repository
    )

    df = read_from_repository_func().where(
        (F.col(DataProductColumnNames.time) >= args.period_start)
        & (F.col(DataProductColumnNames.time) < args.period_end)
    )

    if args.energy_supplier_ids:
        df = df.where(filter_by_energy_supplier_ids(args.energy_supplier_ids))

    if args.calculation_type is CalculationType.BALANCE_FIXING and args.grid_area_codes:
        df = df.where(filter_by_grid_area_codes(args.grid_area_codes))
        df = read_and_filter_by_latest_calculations(
            df=df,
            repository=repository,
            grid_area_codes=args.grid_area_codes,
            period_start=args.period_start,
            period_end=args.period_end,
            time_zone=args.time_zone,
            time_column_name=DataProductColumnNames.time,
        )
    elif args.calculation_id_by_grid_area:
        # args.calculation_id_by_grid_area should never be null when not BALANCE_FIXING.
        df = df.where(
            filter_by_calculation_id_by_grid_area(args.calculation_id_by_grid_area)
        )

    return df

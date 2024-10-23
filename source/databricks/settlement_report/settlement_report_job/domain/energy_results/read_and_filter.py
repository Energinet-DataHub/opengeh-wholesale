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
from uuid import UUID

from pyspark.sql import DataFrame, functions as F

from settlement_report_job import logging
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.wholesale.column_names import DataProductColumnNames
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.infrastructure.calculation_type import CalculationType
from settlement_report_job.wholesale.data_values.calculation_type import (
    CalculationTypeDataProductValue,
)
from settlement_report_job.domain.factory_filters import (
    filter_by_latest_calculations,
    filter_by_energy_supplier_ids,
    filter_by_grid_area_codes,
    filter_by_calculation_id_by_grid_area,
)

log = logging.Logger(__name__)


def _get_view_read_function(
    requesting_actor_market_role: MarketRole,
    repository: WholesaleRepository,
) -> Callable[[], DataFrame]:
    if requesting_actor_market_role == MarketRole.GRID_ACCESS_PROVIDER:
        return repository.read_energy
    else:
        return repository.read_energy_per_es


@logging.use_span()
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
        df = read_and_filter_by_latest_calculations(df, args, repository)
    elif args.calculation_id_by_grid_area:
        df = df.where(
            filter_by_calculation_id_by_grid_area(args.calculation_id_by_grid_area)
        )

    return df


def read_and_filter_by_latest_calculations(
    df: DataFrame, args: SettlementReportArgs, repository: WholesaleRepository
) -> DataFrame:
    latest_balance_fixing_calculations = repository.read_latest_calculations().where(
        (
            F.col(DataProductColumnNames.calculation_type)
            == CalculationTypeDataProductValue.BALANCE_FIXING.value
        )
        & (filter_by_grid_area_codes(args.grid_area_codes))
        & (F.col(DataProductColumnNames.start_of_day) >= args.period_start)
        & (F.col(DataProductColumnNames.start_of_day) < args.period_end)
    )
    df = filter_by_latest_calculations(
        df,
        latest_balance_fixing_calculations,
        df_time_column=DataProductColumnNames.time,
        time_zone=args.time_zone,
    )
    return df

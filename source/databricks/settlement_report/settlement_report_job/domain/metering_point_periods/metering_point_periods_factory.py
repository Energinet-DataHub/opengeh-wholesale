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

from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.infrastructure.repository import WholesaleRepository
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)

from settlement_report_job.domain.metering_point_periods.prepare_for_csv import (
    prepare_for_csv,
)
from settlement_report_job.domain.metering_point_periods.read_and_filter_wholesale import (
    read_and_filter as read_and_filter_wholesale,
)
from settlement_report_job.domain.metering_point_periods.read_and_filter_balance_fixing import (
    read_and_filter as read_and_filter_balance_fixing,
)
from settlement_report_job.entry_points.job_args.calculation_type import CalculationType
from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)


def create_metering_point_periods(
    args: SettlementReportArgs,
    repository: WholesaleRepository,
) -> DataFrame:
    selected_columns = _get_select_columns(args.requesting_actor_market_role)
    if args.calculation_type is CalculationType.BALANCE_FIXING:
        metering_point_periods = read_and_filter_balance_fixing(
            args.period_start,
            args.period_end,
            args.grid_area_codes,
            args.energy_supplier_ids,
            selected_columns,
            args.time_zone,
            repository,
        )
    else:
        metering_point_periods = read_and_filter_wholesale(
            args.period_start,
            args.period_end,
            args.calculation_id_by_grid_area,
            args.energy_supplier_ids,
            args.requesting_actor_market_role,
            args.requesting_actor_id,
            selected_columns,
            repository,
        )

    return prepare_for_csv(
        metering_point_periods,
        args.requesting_actor_market_role,
    )


def _get_select_columns(requesting_actor_market_role: MarketRole) -> list[str]:
    select_columns = [
        DataProductColumnNames.metering_point_id,
        DataProductColumnNames.from_date,
        DataProductColumnNames.to_date,
        DataProductColumnNames.grid_area_code,
        DataProductColumnNames.from_grid_area_code,
        DataProductColumnNames.to_grid_area_code,
        DataProductColumnNames.metering_point_type,
        DataProductColumnNames.settlement_method,
    ]
    if requesting_actor_market_role in [
        MarketRole.SYSTEM_OPERATOR,
        MarketRole.DATAHUB_ADMINISTRATOR,
    ]:
        select_columns.append(DataProductColumnNames.energy_supplier_id)
    return select_columns

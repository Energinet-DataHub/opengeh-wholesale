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
from pyspark.sql.functions import lit, col

from settlement_report_job import logging
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.wholesale.column_names import DataProductColumnNames
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.domain.dataframe_utils.factory_filters import (
    filter_by_calculation_id_by_grid_area,
    filter_by_energy_supplier_ids,
)

log = logging.Logger(__name__)


@logging.use_span()
def read_and_filter_from_view(
    args: SettlementReportArgs, repository: WholesaleRepository
) -> DataFrame:
    monthly_amounts_per_charge = repository.read_monthly_amounts_per_charge_v1()
    monthly_amounts_per_charge = _apply_shared_filters(monthly_amounts_per_charge, args)

    total_monthly_amounts = repository.read_total_monthly_amounts_v1()
    total_monthly_amounts = _filter_total_monthly_amounts(total_monthly_amounts, args)
    total_monthly_amounts = _extend_total_monthly_amounts_columns_for_union(
        total_monthly_amounts,
        monthly_amounts_per_charge_column_ordering=monthly_amounts_per_charge.columns,
    )

    monthly_amounts = monthly_amounts_per_charge.union(total_monthly_amounts)
    monthly_amounts = _extend_monthly_amounts_with_resolution(monthly_amounts)
    monthly_amounts = _drop_columns_based_on_requester(
        monthly_amounts, args.requesting_actor_market_role
    )


def _apply_shared_filters(df: DataFrame, args: SettlementReportArgs) -> DataFrame:
    df = df.where(
        (col(DataProductColumnNames.time) >= args.period_start)
        & (col(DataProductColumnNames.time) < args.period_end)
    )

    if args.calculation_id_by_grid_area:
        # Can never be null, but mypy requires it be specified
        df = df.where(
            filter_by_calculation_id_by_grid_area(args.calculation_id_by_grid_area)
        )

    if args.energy_supplier_ids:
        df = df.where(filter_by_energy_supplier_ids(args.energy_supplier_ids))

    if args.requesting_actor_market_role in [
        MarketRole.GRID_ACCESS_PROVIDER,
        MarketRole.SYSTEM_OPERATOR,
    ]:
        df = df.where(
            col(DataProductColumnNames.charge_owner_id) == args.requesting_actor_id
        )

    return df


def _filter_total_monthly_amounts(
    total_monthly_amounts: DataFrame, args: SettlementReportArgs
) -> DataFrame:
    total_monthly_amounts = _apply_shared_filters(total_monthly_amounts, args)

    if args.requesting_actor_market_role in [
        MarketRole.ENERGY_SUPPLIER,
    ]:
        total_monthly_amounts = total_monthly_amounts.where(
            col(DataProductColumnNames.charge_owner_id).isNull()
        )

    return total_monthly_amounts


def _extend_monthly_amounts_with_resolution(
    base_monthly_amounts_per_charge_columns: DataFrame,
) -> DataFrame:
    return base_monthly_amounts_per_charge_columns.withColumn(
        DataProductColumnNames.resolution, lit("P1M")
    )


def _extend_total_monthly_amounts_columns_for_union(
    base_total_monthly_amounts: DataFrame,
    monthly_amounts_per_charge_column_ordering: list[str],
) -> DataFrame:
    for null_column in [
        DataProductColumnNames.quantity_unit,
        DataProductColumnNames.charge_type,
        DataProductColumnNames.charge_code,
        DataProductColumnNames.is_tax,
    ]:
        base_total_monthly_amounts = base_total_monthly_amounts.withColumn(
            null_column, lit(None)
        )

    return base_total_monthly_amounts.select(
        monthly_amounts_per_charge_column_ordering,
    )


def _drop_columns_based_on_requester(
    monthly_amounts: DataFrame, market_role: MarketRole
):
    if market_role in [MarketRole.GRID_ACCESS_PROVIDER, MarketRole.SYSTEM_OPERATOR]:
        monthly_amounts = monthly_amounts.drop(DataProductColumnNames.charge_owner_id)
    return monthly_amounts

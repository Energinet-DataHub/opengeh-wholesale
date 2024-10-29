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
from pyspark.sql import DataFrame, functions as F

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
    monthly_amounts_per_charge = extend_monthly_amounts_per_charge_columns_for_union(
        monthly_amounts_per_charge
    )
    monthly_amounts_per_charge = filter_monthly_amounts_per_charge(
        monthly_amounts_per_charge, args
    )

    total_monthly_amounts = repository.read_total_monthly_amounts_v1()
    total_monthly_amounts = extend_total_monthly_amounts_columns_for_union(
        total_monthly_amounts
    )
    total_monthly_amounts = filter_total_monthly_amounts(total_monthly_amounts, args)

    return monthly_amounts_per_charge.union(total_monthly_amounts)


def apply_shared_filters(df: DataFrame, args: SettlementReportArgs) -> DataFrame:
    df = df.where(
        (F.col(DataProductColumnNames.time) >= args.period_start)
        & (F.col(DataProductColumnNames.time) < args.period_end)
    )

    if args.calculation_id_by_grid_area:
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
            F.col(DataProductColumnNames.charge_owner_id) == args.requesting_actor_id
        )

    return df


def filter_monthly_amounts_per_charge(
    monthly_amounts_per_charge: DataFrame, args: SettlementReportArgs
) -> DataFrame:
    monthly_amounts_per_charge = apply_shared_filters(monthly_amounts_per_charge, args)

    return monthly_amounts_per_charge.where(
        (F.col(DataProductColumnNames.charge_owner_id).isNotNull())
        & (F.col(DataProductColumnNames.charge_code).isNotNull())
        & (F.col(DataProductColumnNames.charge_type).isNotNull())
        & (F.col(DataProductColumnNames.is_tax).isNotNull())
    )


def filter_total_monthly_amounts(
    total_monthly_amounts: DataFrame, args: SettlementReportArgs
) -> DataFrame:
    total_monthly_amounts = apply_shared_filters(total_monthly_amounts, args)

    if args.requesting_actor_market_role in [
        MarketRole.ENERGY_SUPPLIER,
        MarketRole.DATAHUB_ADMINISTRATOR,
    ]:
        total_monthly_amounts = total_monthly_amounts.where(
            F.col(DataProductColumnNames.charge_owner_id).isNull()
        )

    return total_monthly_amounts


def extend_monthly_amounts_per_charge_columns_for_union(
    base_monthly_amounts_per_charge_columns: DataFrame,
) -> DataFrame:
    return base_monthly_amounts_per_charge_columns.select(
        F.col(DataProductColumnNames.calculation_id),
        F.col(DataProductColumnNames.calculation_type),
        F.col(DataProductColumnNames.calculation_version),
        F.col(DataProductColumnNames.result_id),
        F.col(DataProductColumnNames.grid_area_code),
        F.col(DataProductColumnNames.energy_supplier_id),
        F.col(DataProductColumnNames.time),
        F.lit("P1M").alias(DataProductColumnNames.resolution),
        F.col(DataProductColumnNames.quantity_unit),
        F.lit("DKK").alias(DataProductColumnNames.currency),
        F.col(DataProductColumnNames.amount),
        F.col(DataProductColumnNames.charge_type),
        F.col(DataProductColumnNames.charge_code),
        F.col(DataProductColumnNames.charge_owner_id),
        F.col(DataProductColumnNames.is_tax),
    )


def extend_total_monthly_amounts_columns_for_union(
    base_total_monthly_amounts: DataFrame,
) -> DataFrame:
    return base_total_monthly_amounts.select(
        F.col(DataProductColumnNames.calculation_id),
        F.col(DataProductColumnNames.calculation_type),
        F.col(DataProductColumnNames.calculation_version),
        F.col(DataProductColumnNames.result_id),
        F.col(DataProductColumnNames.grid_area_code),
        F.col(DataProductColumnNames.energy_supplier_id),
        F.col(DataProductColumnNames.time),
        F.lit("P1M").alias(DataProductColumnNames.resolution),
        F.lit(None).alias(DataProductColumnNames.quantity_unit),
        F.lit("DKK").alias(DataProductColumnNames.currency),
        F.col(DataProductColumnNames.amount),
        F.lit(None).alias(DataProductColumnNames.charge_type),
        F.lit(None).alias(DataProductColumnNames.charge_code),
        F.col(DataProductColumnNames.charge_owner_id),
        F.lit(None).alias(DataProductColumnNames.is_tax),
    )

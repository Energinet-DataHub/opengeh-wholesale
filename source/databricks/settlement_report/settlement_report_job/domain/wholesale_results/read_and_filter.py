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

from settlement_report_job import logging
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.wholesale.column_names import DataProductColumnNames
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs

log = logging.Logger(__name__)


def _get_view_read_function(
    repository: WholesaleRepository,
) -> Callable[[], DataFrame]:
    return repository.read_wholesale_results


@logging.use_span()
def read_and_filter_from_view(
    args: SettlementReportArgs, repository: WholesaleRepository
) -> DataFrame:
    read_from_repository_func = _get_view_read_function(repository)

    df = read_from_repository_func().where(
        (F.col(DataProductColumnNames.time) >= args.period_start)
        & (F.col(DataProductColumnNames.time) < args.period_end)
    )

    if args.energy_supplier_ids is not None:
        df = df.where(
            F.col(DataProductColumnNames.energy_supplier_id).isin(
                args.energy_supplier_ids
            )
        )

    if args.calculation_id_by_grid_area is not None:
        calculation_id_by_grid_area_structs = [
            F.struct(F.lit(grid_area_code), F.lit(str(calculation_id)))
            for grid_area_code, calculation_id in args.calculation_id_by_grid_area.items()
        ]

        df = df.where(
            F.struct(
                F.col(DataProductColumnNames.grid_area_code),
                F.col(DataProductColumnNames.calculation_id),
            ).isin(calculation_id_by_grid_area_structs)
        )

    return df

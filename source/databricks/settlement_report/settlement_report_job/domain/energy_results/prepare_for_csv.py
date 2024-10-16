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

from pyspark.sql import DataFrame, functions as F, Window

from settlement_report_job import logging
from settlement_report_job.domain.get_start_of_day import (
    get_start_of_day,
)
from settlement_report_job.domain.report_naming_convention import (
    METERING_POINT_TYPES,
)
from settlement_report_job.domain.csv_column_names import (
    EnergyResultsCsvColumnNames,
)
from settlement_report_job.utils import (
    map_from_dict,
)
from settlement_report_job.wholesale.column_names import DataProductColumnNames
from settlement_report_job.wholesale.data_values import (
    MeteringPointResolutionDataProductValue,
)
import settlement_report_job.domain.report_naming_convention as market_naming

log = logging.Logger(__name__)


@logging.use_span()
def prepare_for_csv(
    energy: DataFrame,
) -> DataFrame:
    select_columns = [
        F.col(DataProductColumnNames.grid_area_code).alias(
            EnergyResultsCsvColumnNames.grid_area_code
        ),
        map_from_dict(market_naming.CALCULATION_TYPES_TO_ENERGY_BUSINESS_PROCESS)[
            F.col(DataProductColumnNames.calculation_type)
        ].alias(EnergyResultsCsvColumnNames.calculation_type),
        F.col(DataProductColumnNames.time).alias(EnergyResultsCsvColumnNames.time),
        F.col(DataProductColumnNames.resolution).alias(
            EnergyResultsCsvColumnNames.resolution
        ),
        map_from_dict(market_naming.METERING_POINT_TYPES)[
            F.col(DataProductColumnNames.metering_point_type)
        ].alias(EnergyResultsCsvColumnNames.metering_point_type),
        map_from_dict(market_naming.SETTLEMENT_METHODS)[
            F.col(DataProductColumnNames.settlement_method)
        ].alias(EnergyResultsCsvColumnNames.settlement_method),
        F.col(DataProductColumnNames.quantity).alias(
            EnergyResultsCsvColumnNames.quantity
        ),
<<<<<<< HEAD:source/databricks/settlement_report/settlement_report_job/domain/energy_results/prepare_for_csv.py
=======
    )


def _read_and_filter_from_view(
    args: SettlementReportArgs, repository: WholesaleRepository
) -> DataFrame:
    df = repository.read_energy().where(
        (F.col(DataProductColumnNames.time) >= args.period_start)
        & (F.col(DataProductColumnNames.time) < args.period_end)
    )

    calculation_id_by_grid_area_structs = [
        F.struct(F.lit(grid_area_code), F.lit(str(calculation_id)))
        for grid_area_code, calculation_id in args.calculation_id_by_grid_area.items()  # type: ignore[union-attr]
>>>>>>> main:source/databricks/settlement_report/settlement_report_job/domain/energy_results_factory.py
    ]

    if DataProductColumnNames.energy_supplier_id in energy.columns:
        select_columns.insert(
            1,
            F.col(DataProductColumnNames.energy_supplier_id).alias(
                EnergyResultsCsvColumnNames.energy_supplier_id
            ),
        )

    return energy.select(select_columns)

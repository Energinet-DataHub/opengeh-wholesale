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
from settlement_report_job.domain.csv_column_names import (
    CsvColumnNames,
)
from settlement_report_job.utils import (
    map_from_dict,
)
from settlement_report_job.wholesale.column_names import DataProductColumnNames
import settlement_report_job.domain.report_naming_convention as market_naming

log = logging.Logger(__name__)


@logging.use_span()
def prepare_for_csv(
    wholesale: DataFrame,
) -> DataFrame:
    select_columns = [
        map_from_dict(market_naming.CALCULATION_TYPES_TO_ENERGY_BUSINESS_PROCESS)[
            F.col(DataProductColumnNames.calculation_type)
        ].alias(CsvColumnNames.energy_business_process),
        map_from_dict(market_naming.CALCULATION_TYPES_TO_PROCESS_VARIANT)[
            F.col(DataProductColumnNames.calculation_type)
        ].alias(CsvColumnNames.process_variant),
        F.col(DataProductColumnNames.grid_area_code).alias(
            CsvColumnNames.metering_grid_area
        ),
        F.col(DataProductColumnNames.energy_supplier_id).alias(
            CsvColumnNames.energy_supplier_id
        ),
        F.col(DataProductColumnNames.time).alias(CsvColumnNames.start_date_time),
        F.col(DataProductColumnNames.resolution).alias(
            CsvColumnNames.resolution_duration
        ),
        map_from_dict(market_naming.METERING_POINT_TYPES)[
            F.col(DataProductColumnNames.metering_point_type)
        ].alias(CsvColumnNames.type_of_mp),
        map_from_dict(market_naming.SETTLEMENT_METHODS)[
            F.col(DataProductColumnNames.settlement_method)
        ].alias(CsvColumnNames.settlement_method),
        F.col(DataProductColumnNames.quantity_unit).alias(CsvColumnNames.measure_unit),
        F.col(DataProductColumnNames.currency).alias(CsvColumnNames.energy_currency),
        F.col(DataProductColumnNames.quantity).alias(CsvColumnNames.energy_quantity),
        F.col(DataProductColumnNames.price).alias(CsvColumnNames.price),
        F.col(DataProductColumnNames.amount).alias(CsvColumnNames.amount),
        map_from_dict(market_naming.CHARGE_TYPES)[
            F.col(DataProductColumnNames.charge_type)
        ].alias(CsvColumnNames.charge_type),
        F.col(DataProductColumnNames.charge_code).alias(CsvColumnNames.charge_id),
        F.col(DataProductColumnNames.charge_owner_id).alias(
            CsvColumnNames.charge_owner
        ),
    ]

    return wholesale.select(select_columns).orderBy(
        F.col(CsvColumnNames.metering_grid_area),
        F.col(CsvColumnNames.energy_supplier_id),
        F.col(CsvColumnNames.type_of_mp),
        F.col(CsvColumnNames.settlement_method),
        F.col(CsvColumnNames.start_date_time),
        F.col(CsvColumnNames.charge_owner),
        F.col(CsvColumnNames.charge_type),
        F.col(CsvColumnNames.charge_id),
    )

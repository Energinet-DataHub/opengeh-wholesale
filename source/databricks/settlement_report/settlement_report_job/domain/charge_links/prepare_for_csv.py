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

from telemetry_logging import Logger, use_span
from settlement_report_job.domain.utils.map_to_csv_naming import (
    METERING_POINT_TYPES,
    CHARGE_TYPES,
)
from settlement_report_job.domain.utils.csv_column_names import (
    CsvColumnNames,
    EphemeralColumns,
)
from settlement_report_job.domain.utils.map_from_dict import (
    map_from_dict,
)
from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)

log = Logger(__name__)


@use_span()
def prepare_for_csv(
    charge_link_periods: DataFrame,
) -> DataFrame:
    columns = [
        F.col(DataProductColumnNames.grid_area_code).alias(
            EphemeralColumns.grid_area_code_partitioning
        ),
        F.col(DataProductColumnNames.metering_point_id).alias(
            CsvColumnNames.metering_point_id
        ),
        map_from_dict(METERING_POINT_TYPES)[
            F.col(DataProductColumnNames.metering_point_type)
        ].alias(CsvColumnNames.metering_point_type),
        map_from_dict(CHARGE_TYPES)[F.col(DataProductColumnNames.charge_type)].alias(
            CsvColumnNames.charge_type
        ),
        F.col(DataProductColumnNames.charge_owner_id).alias(
            CsvColumnNames.charge_owner_id
        ),
        F.col(DataProductColumnNames.charge_code).alias(CsvColumnNames.charge_code),
        F.col(DataProductColumnNames.quantity).alias(CsvColumnNames.charge_quantity),
        F.col(DataProductColumnNames.from_date).alias(
            CsvColumnNames.charge_link_from_date
        ),
        F.col(DataProductColumnNames.to_date).alias(CsvColumnNames.charge_link_to_date),
    ]
    if DataProductColumnNames.energy_supplier_id in charge_link_periods.columns:
        columns.append(
            F.col(DataProductColumnNames.energy_supplier_id).alias(
                CsvColumnNames.energy_supplier_id
            )
        )

    return charge_link_periods.select(columns)

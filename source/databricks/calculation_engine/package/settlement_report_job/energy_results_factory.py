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
from pyspark.sql.session import SparkSession
import pyspark.sql.functions as F

from package.settlement_report_job.constants import get_energy_view
from package.settlement_report_job.settlement_report_args import SettlementReportArgs
from package.settlement_report_job.table_column_names import (
    DataProductColumnNames,
    EnergyResultsCsvColumnNames,
)


def create_energy_results(
    spark: SparkSession,
    args: SettlementReportArgs,
) -> DataFrame:
    calculation_id_by_grid_area_df = spark.createDataFrame(
        args.calculation_id_by_grid_area.items(),
        [DataProductColumnNames.grid_area_code, DataProductColumnNames.calculation_id],
    )

    energy = spark.read.table(get_energy_view())

    energy_filtered = energy.join(
        calculation_id_by_grid_area_df,
        [DataProductColumnNames.grid_area_code, DataProductColumnNames.calculation_id],
    ).where(
        (F.col(DataProductColumnNames.time) >= args.period_start)
        & (F.col(DataProductColumnNames.time) < args.period_end)
    )

    return energy_filtered.select(
        F.col(DataProductColumnNames.grid_area_code).alias(
            EnergyResultsCsvColumnNames.grid_area_code
        ),
        F.col(DataProductColumnNames.calculation_type).alias(
            EnergyResultsCsvColumnNames.calculation_type
        ),
        F.col(DataProductColumnNames.time).alias(EnergyResultsCsvColumnNames.time),
        F.col(DataProductColumnNames.resolution).alias(
            EnergyResultsCsvColumnNames.resolution
        ),
        F.col(DataProductColumnNames.metering_point_type).alias(
            EnergyResultsCsvColumnNames.metering_point_type
        ),
        F.col(DataProductColumnNames.settlement_method).alias(
            EnergyResultsCsvColumnNames.settlement_method
        ),
        F.col(DataProductColumnNames.quantity).alias(
            EnergyResultsCsvColumnNames.quantity
        ),
    )

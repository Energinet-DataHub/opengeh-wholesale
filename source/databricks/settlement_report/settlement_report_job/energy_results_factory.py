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


import settlement_report_job.market_naming_convention as market_naming
from settlement_report_job.settlement_report_args import SettlementReportArgs

from settlement_report_job.table_column_names import (
    EnergyResultsCsvColumnNames,
    DataProductColumnNames,
)
from settlement_report_job.utils import map_from_dict


def create_energy_results(
    spark: SparkSession,
    args: SettlementReportArgs,
) -> DataFrame:

    energy = _read_from_view(spark, args, get_energy_view())

    # return relevant columns with market naming convention
    return energy.select(
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
        map_from_dict(market_naming.METERING_POINT_TYPE_DICT)[
            F.col(DataProductColumnNames.metering_point_type)
        ].alias(EnergyResultsCsvColumnNames.metering_point_type),
        map_from_dict(market_naming.SETTLEMENT_METHOD_DICT)[
            F.col(DataProductColumnNames.settlement_method)
        ].alias(EnergyResultsCsvColumnNames.settlement_method),
        F.col(DataProductColumnNames.quantity).alias(
            EnergyResultsCsvColumnNames.quantity
        ),
    )


def _read_from_view(
    spark: SparkSession, args: SettlementReportArgs, view_name: str
) -> DataFrame:
    df = spark.read.table(view_name).where(
        F.col(DataProductColumnNames.time) >= args.period_start
    ) & (F.col(DataProductColumnNames.time) < args.period_end)

    calculation_id_by_grid_area_structs = [
        F.struct(F.lit(grid_area_code), F.lit(calculation_id))
        for grid_area_code, calculation_id in args.calculation_id_by_grid_area
    ]

    df_filtered = df.where(
        F.struct(
            F.col(DataProductColumnNames.grid_area_code),
            F.col(DataProductColumnNames.calculation_id),
        ).isin(calculation_id_by_grid_area_structs)
    )

    return df_filtered

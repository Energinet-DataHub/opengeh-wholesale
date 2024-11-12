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
from settlement_report_job.domain.report_naming_convention import (
    CHARGE_TYPES,
)
from settlement_report_job.domain.csv_column_names import (
    CsvColumnNames,
)
from settlement_report_job.utils import map_from_dict
from settlement_report_job.wholesale.column_names import DataProductColumnNames
from settlement_report_job.wholesale.data_values import ChargeResolutionDataProductValue

log = Logger(__name__)


@use_span()
def prepare_for_csv(
    charge_prices: DataFrame,
) -> DataFrame:
    columns = [
        map_from_dict(CHARGE_TYPES)[F.col(DataProductColumnNames.charge_type)].alias(
            CsvColumnNames.charge_type
        ),
        F.col(DataProductColumnNames.charge_owner_id).alias(
            CsvColumnNames.charge_owner_id
        ),
        F.col(DataProductColumnNames.charge_code).alias(CsvColumnNames.charge_code),
        F.col(DataProductColumnNames.resolution).alias(CsvColumnNames.resolution),
        F.col(DataProductColumnNames.is_tax).alias(CsvColumnNames.is_tax),
        F.col(DataProductColumnNames.charge_time).alias(CsvColumnNames.time),
        F.col(DataProductColumnNames.charge_price).alias("ENERGYPRICE1"),
        F.lit(None).alias("ENERGYPRICE2"),
        F.lit(None).alias("ENERGYPRICE3"),
        F.lit(None).alias("ENERGYPRICE4"),
        F.lit(None).alias("ENERGYPRICE5"),
        F.lit(None).alias("ENERGYPRICE6"),
        F.lit(None).alias("ENERGYPRICE7"),
        F.lit(None).alias("ENERGYPRICE8"),
        F.lit(None).alias("ENERGYPRICE9"),
        F.lit(None).alias("ENERGYPRICE10"),
        F.lit(None).alias("ENERGYPRICE11"),
        F.lit(None).alias("ENERGYPRICE12"),
        F.lit(None).alias("ENERGYPRICE13"),
        F.lit(None).alias("ENERGYPRICE14"),
        F.lit(None).alias("ENERGYPRICE15"),
        F.lit(None).alias("ENERGYPRICE16"),
        F.lit(None).alias("ENERGYPRICE17"),
        F.lit(None).alias("ENERGYPRICE18"),
        F.lit(None).alias("ENERGYPRICE19"),
        F.lit(None).alias("ENERGYPRICE20"),
        F.lit(None).alias("ENERGYPRICE21"),
        F.lit(None).alias("ENERGYPRICE22"),
        F.lit(None).alias("ENERGYPRICE23"),
        F.lit(None).alias("ENERGYPRICE24"),
        F.lit(None).alias("ENERGYPRICE25"),
    ]
    charge_prices = charge_prices.select(columns)

    hourly_charge_prices = charge_prices.filter(
        F.col(DataProductColumnNames.resolution)
        == ChargeResolutionDataProductValue.HOUR.value
    )

    charge_prices_without_hourly_resolution = charge_prices.filter(
        F.col(DataProductColumnNames.resolution)
        != ChargeResolutionDataProductValue.HOUR.value
    )

    for i in range(2, 26):
        hourly_charge_prices = hourly_charge_prices.withColumn(
            f"ENERGYPRICE{i}",
            F.col("ENERGYPRICE1"),
        )

    charge_prices = charge_prices_without_hourly_resolution.union(hourly_charge_prices)

    return charge_prices

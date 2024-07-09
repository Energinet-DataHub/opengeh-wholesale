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
from dependency_injector.wiring import inject, Provide
from pyspark.sql import DataFrame

from package.calculation.calculation_results import (
    WholesaleResultsContainer,
)
from package.calculation.output.output_table_column_names import OutputTableColumnNames
from package.constants import TotalMonthlyAmountsColumnNames
from package.container import Container
from package.infrastructure import logging_configuration
from package.infrastructure.infrastructure_settings import InfrastructureSettings
from package.infrastructure.paths import (
    HiveOutputDatabase,
    WholesaleResultsInternalDatabase,
)


@logging_configuration.use_span("calculation.write.wholesale")
def write_total_monthly_amounts(
    total_monthly_amounts: WholesaleResultsContainer,
) -> None:
    _write(
        "total_monthly_amounts_per_co_es",
        total_monthly_amounts.total_monthly_amounts_per_co_es,
    )
    _write(
        "total_monthly_amounts_per_es",
        total_monthly_amounts.total_monthly_amounts_per_es,
    )


@inject
def _write(
    name: str,
    df: DataFrame,
    infrastructure_settings: InfrastructureSettings = Provide[
        Container.infrastructure_settings
    ],
) -> None:
    with logging_configuration.start_span(name):
        df.drop(
            # ToDo JMG: Remove when we are on Unity Catalog
            TotalMonthlyAmountsColumnNames.calculation_type,
            TotalMonthlyAmountsColumnNames.calculation_execution_time_start,
        ).withColumnRenamed(
            # ToDo JMG: Remove when we are on Unity Catalog
            TotalMonthlyAmountsColumnNames.calculation_result_id,
            OutputTableColumnNames.result_id,
        ).write.format(
            "delta"
        ).mode(
            "append"
        ).option(
            "mergeSchema", "false"
        ).insertInto(
            f"{infrastructure_settings.catalog_name}.{WholesaleResultsInternalDatabase.DATABASE_NAME}.{WholesaleResultsInternalDatabase.TOTAL_MONTHLY_AMOUNTS_TABLE_NAME}"
        )

        # ToDo JMG: Remove when we are on Unity Catalog
        df.write.format("delta").mode("append").option(
            "mergeSchema", "false"
        ).insertInto(
            f"{HiveOutputDatabase.DATABASE_NAME}.{HiveOutputDatabase.TOTAL_MONTHLY_AMOUNTS_TABLE_NAME}"
        )

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
from pyspark.sql.functions import current_timestamp

from package.container import Container
from package.databases.table_column_names import TableColumnNames
from package.infrastructure import logging_configuration
from package.infrastructure.infrastructure_settings import InfrastructureSettings
from package.infrastructure.paths import (
    HiveBasisDataDatabase,
    WholesaleInternalDatabase,
)


@logging_configuration.use_span("calculation.write-succeeded-calculation")
@inject
def write_calculation(
    calculations: DataFrame,
    infrastructure_settings: InfrastructureSettings = Provide[
        Container.infrastructure_settings
    ],
) -> None:
    """Writes the succeeded calculation to the calculations table."""

    # create a timestamp with current time

    calculations = calculations.withColumn(
        TableColumnNames.calculation_completed_time, current_timestamp()
    )

    calculations.write.format("delta").mode("append").option(
        "mergeSchema", "false"
    ).insertInto(
        f"{infrastructure_settings.catalog_name}.{WholesaleInternalDatabase.DATABASE_NAME}.{WholesaleInternalDatabase.CALCULATIONS_TABLE_NAME}"
    )

    # ToDo JMG: Remove when we are on Unity Catalog
    calculations.write.format("delta").mode("append").option(
        "mergeSchema", "false"
    ).insertInto(
        f"{HiveBasisDataDatabase.DATABASE_NAME}.{HiveBasisDataDatabase.CALCULATIONS_TABLE_NAME}"
    )


@logging_configuration.use_span("calculation.write-calculation-grid-areas")
@inject
def write_calculation_grid_areas(
    calculations_grid_areas: DataFrame,
    infrastructure_settings: InfrastructureSettings = Provide[
        Container.infrastructure_settings
    ],
) -> None:
    """Writes the calculation grid areas to the calculation grid areas table."""

    calculations_grid_areas.write.format("delta").mode("append").option(
        "mergeSchema", "false"
    ).insertInto(
        f"{infrastructure_settings.catalog_name}.{WholesaleInternalDatabase.DATABASE_NAME}.{WholesaleInternalDatabase.CALCULATION_GRID_AREAS_TABLE_NAME}"
    )

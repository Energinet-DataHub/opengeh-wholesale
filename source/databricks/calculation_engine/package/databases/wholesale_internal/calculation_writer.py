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
from pyspark.sql import DataFrame, SparkSession
from delta.exceptions import MetadataChangedException

from package.calculation.calculator_args import CalculatorArgs
from package.container import Container
from package.databases.table_column_names import TableColumnNames
from package.infrastructure import logging_configuration
from package.infrastructure.infrastructure_settings import InfrastructureSettings
from package.infrastructure.paths import (
    WholesaleInternalDatabase,
)

timestamp_format = "%Y-%m-%dT%H:%M:%S.%f"
METADATA_CHANGED_RETRIES = 10


@logging_configuration.use_span("calculation.write-succeeded-calculation")
@inject
def write_calculation(
    args: CalculatorArgs,
    spark: SparkSession = Provide[Container.spark],
    infrastructure_settings: InfrastructureSettings = Provide[
        Container.infrastructure_settings
    ],
) -> None:
    """Writes the succeeded calculation to the calculations table. The current time is  added to the calculation before writing."""
    calculation_period_start_datetime = args.calculation_period_start_datetime.strftime(
        timestamp_format
    )[:-3]

    calculation_period_end_datetime = args.calculation_period_end_datetime.strftime(
        timestamp_format
    )[:-3]
    calculation_execution_time_start = args.calculation_execution_time_start.strftime(
        timestamp_format
    )[:-3]
    # We had to use sql statement to insert the data because the DataFrame.write.insertInto() method does not support IDENTITY columns
    # Also, since IDENTITY COLUMN requires an exclusive lock on the table, we allow up to METADATA_CHANGED_RETRIES retries of the transaction.
    for attempt in range(METADATA_CHANGED_RETRIES):
        try:
            spark.sql(
                f"INSERT INTO {infrastructure_settings.catalog_name}.{WholesaleInternalDatabase.DATABASE_NAME}.{WholesaleInternalDatabase.CALCULATIONS_TABLE_NAME}"
                f" ({TableColumnNames.calculation_id}, {TableColumnNames.calculation_type}, {TableColumnNames.calculation_period_start}, {TableColumnNames.calculation_period_end}, {TableColumnNames.calculation_execution_time_start}, {TableColumnNames.calculation_succeeded_time}, {TableColumnNames.is_internal_calculation})"
                f" VALUES ('{args.calculation_id}', '{args.calculation_type.value}', '{calculation_period_start_datetime}', '{calculation_period_end_datetime}', '{calculation_execution_time_start}', NULL, '{args.is_internal_calculation}');"
            )
            break
        except MetadataChangedException as e:
            if attempt == METADATA_CHANGED_RETRIES:
                raise e
            else:
                spark.catalog.uncacheTable(
                    f"{infrastructure_settings.catalog_name}.{WholesaleInternalDatabase.DATABASE_NAME}.{WholesaleInternalDatabase.CALCULATIONS_TABLE_NAME}"
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


def write_calculation_succeeded_time(
    calculation_id: str,
    spark: SparkSession = Provide[Container.spark],
    infrastructure_settings: InfrastructureSettings = Provide[
        Container.infrastructure_settings
    ],
) -> None:
    """Writes the succeeded time to the calculation table."""

    spark.sql(
        f"""
        UPDATE {infrastructure_settings.catalog_name}.{WholesaleInternalDatabase.DATABASE_NAME}.{WholesaleInternalDatabase.CALCULATIONS_TABLE_NAME}
        SET {TableColumnNames.calculation_succeeded_time} = current_timestamp()
        WHERE {TableColumnNames.calculation_id} = '{calculation_id}'
        """
    )

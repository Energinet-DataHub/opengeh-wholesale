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
import argparse
from datetime import datetime

import pytest
from pyspark import Row
from pyspark.sql import SparkSession, DataFrame

from package.calculation import PreparedDataReader, calculation, CalculationCore
from package.calculation.calculation_metadata_service import CalculationMetadataService
from package.calculation.calculation_output_service import CalculationOutputService
from package.calculation.calculator_args import CalculatorArgs
from package.calculator_job import start_with_deps
from package.databases import migrations_wholesale, wholesale_internal
from package.databases.wholesale_internal.schemas import calculations_schema
from package.infrastructure.infrastructure_settings import InfrastructureSettings
from package.infrastructure.paths import WholesaleInternalDatabase


@pytest.mark.parametrize(
    "calculation_id,test_successful",
    [
        ("0b15a420-9fc8-409a-a169-fbd49479d718", False),
        ("0b15a420-9fc8-409a-a169-fbd49479d719", True),
    ],
)
def test_(
    calculator_args_balance_fixing: CalculatorArgs,
    calculation_input_database: str,
    spark: SparkSession,
    any_calculator_args: CalculatorArgs,
    infrastructure_settings: InfrastructureSettings,
    calculation_id: str,
    test_successful: bool,
) -> None:

    # Arrange
    migrations_wholesale_repository = (
        migrations_wholesale.MigrationsWholesaleRepository(
            spark, "spark_catalog", calculation_input_database
        )
    )
    wholesale_internal_repository = wholesale_internal.WholesaleInternalRepository(
        spark, "spark_catalog"
    )

    prepared_data_reader = PreparedDataReader(
        migrations_wholesale_repository, wholesale_internal_repository
    )

    command_line_args = argparse.Namespace()
    command_line_args.calculation_id = calculation_id
    any_calculator_args.calculation_id = command_line_args.calculation_id

    data = [
        Row(
            calculation_id="0b15a420-9fc8-409a-a169-fbd49479d718",
            calculation_type="balance_fixing",
            calculation_period_start=datetime.now(),
            calculation_period_end=datetime.now(),
            calculation_execution_time_start=datetime.now(),
            created_by_user_id="0b15a420-9fc8-409a-a169-fbd49479d718",
            calculation_version=1,
            is_internal_calculation=True,
            calculation_succeeded_time=datetime.now(),
        )
    ]

    calculations_df = spark.createDataFrame(data, calculations_schema)
    write_calculation(calculations_df, infrastructure_settings)

    # Act
    try:
        start_with_deps(
            parse_command_line_args=lambda: command_line_args,
            parse_job_args=lambda args: (any_calculator_args, infrastructure_settings),
            calculation_executor=lambda *args: calculation.execute(
                any_calculator_args,
                prepared_data_reader,
                CalculationCore(),
                CalculationMetadataService(),
                CalculationOutputService(),
            ),
        )
    except SystemExit as e:
        # Assert
        assert e.code == 4 and not test_successful
        print(e.args)


def write_calculation(
    df: DataFrame, infrastructure_settings: InfrastructureSettings
) -> None:
    # Read the existing table
    df = spark.read.format("delta").table(table_name)

    # Filter out the rows with the specified calculation ID
    filtered_df = df.filter(df.calculation_id != calculation_id)

    # Overwrite the table with the filtered DataFrame
    filtered_df.write.format("delta").mode("overwrite").saveAsTable(table_name)

    df.write.format("delta").mode("append").saveAsTable(
        f"{infrastructure_settings.catalog_name}.{WholesaleInternalDatabase.DATABASE_NAME}.{WholesaleInternalDatabase.CALCULATIONS_TABLE_NAME}"
    )

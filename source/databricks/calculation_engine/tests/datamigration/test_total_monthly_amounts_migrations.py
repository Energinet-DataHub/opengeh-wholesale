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

from datetime import datetime
from decimal import Decimal
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import col, lit
import pytest
import uuid

from contract_utils import assert_contract_matches_schema
from package.calculation.output.schemas.total_monthly_amounts_schema import (
    total_monthly_amounts_schema,
)
from tests.helpers.data_frame_utils import set_column
from package.codelists import CalculationType
from package.constants import TotalMonthlyAmountsColumnNames
from package.infrastructure.paths import (
    OUTPUT_DATABASE_NAME,
    TOTAL_MONTHLY_AMOUNTS_TABLE_NAME,
)


def _create_df(spark: SparkSession) -> DataFrame:
    row = {
        TotalMonthlyAmountsColumnNames.calculation_id: "9252d7a0-4363-42cc-a2d6-e04c026523f8",
        TotalMonthlyAmountsColumnNames.calculation_type: "WholesaleFixing",
        TotalMonthlyAmountsColumnNames.calculation_execution_time_start: datetime(
            2020, 1, 1, 0, 0
        ),
        TotalMonthlyAmountsColumnNames.calculation_result_id: "6033ab5c-436b-44e9-8a79-90489d324e53",
        TotalMonthlyAmountsColumnNames.grid_area: "543",
        TotalMonthlyAmountsColumnNames.energy_supplier_id: "1234567890123",
        TotalMonthlyAmountsColumnNames.time: datetime(2020, 1, 1, 0, 0),
        TotalMonthlyAmountsColumnNames.amount: Decimal("1.123"),
        TotalMonthlyAmountsColumnNames.charge_owner_id: "1234567890123",
    }
    return spark.createDataFrame(data=[row], schema=total_monthly_amounts_schema)


def test__migrated_table__columns_matching_contract(
    spark: SparkSession,
    contracts_path: str,
    migrations_executed: None,
) -> None:
    # Arrange
    contract_path = f"{contracts_path}/total-monthly-amounts-table-column-names.json"

    # Act
    actual = spark.table(
        f"{OUTPUT_DATABASE_NAME}.{TOTAL_MONTHLY_AMOUNTS_TABLE_NAME}"
    ).schema

    # Assert
    assert_contract_matches_schema(contract_path, actual)


@pytest.mark.parametrize(
    "column_name,invalid_column_value",
    [
        (TotalMonthlyAmountsColumnNames.calculation_id, None),
        (TotalMonthlyAmountsColumnNames.calculation_id, "not-a-uuid"),
        (TotalMonthlyAmountsColumnNames.calculation_type, None),
        (TotalMonthlyAmountsColumnNames.calculation_type, "foo"),
        (TotalMonthlyAmountsColumnNames.calculation_execution_time_start, None),
        (TotalMonthlyAmountsColumnNames.calculation_result_id, None),
        (TotalMonthlyAmountsColumnNames.calculation_result_id, "not-a-uuid"),
        (TotalMonthlyAmountsColumnNames.grid_area, None),
        (TotalMonthlyAmountsColumnNames.grid_area, "12"),
        (TotalMonthlyAmountsColumnNames.grid_area, "1234"),
        (
            TotalMonthlyAmountsColumnNames.energy_supplier_id,
            "neither-16-nor-13-digits-long",
        ),
        (TotalMonthlyAmountsColumnNames.energy_supplier_id, None),
        (TotalMonthlyAmountsColumnNames.time, None),
        (
            TotalMonthlyAmountsColumnNames.charge_owner_id,
            "neither-16-nor-13-digits-long",
        ),
    ],
)
def test__migrated_table_rejects_invalid_data(
    spark: SparkSession,
    column_name: str,
    invalid_column_value: str | list,
    migrations_executed: None,
) -> None:
    # Arrange
    results_df = _create_df(spark)

    invalid_df = set_column(results_df, column_name, invalid_column_value)

    # Act
    with pytest.raises(Exception) as ex:
        invalid_df.write.format("delta").option("mergeSchema", "false").insertInto(
            f"{OUTPUT_DATABASE_NAME}.{TOTAL_MONTHLY_AMOUNTS_TABLE_NAME}",
            overwrite=False,
        )

    # Assert: Do sufficient assertions to be confident that the expected violation has been caught
    actual_error_message = str(ex.value)
    assert "DeltaInvariantViolationException" in actual_error_message
    assert column_name in actual_error_message


min_18_6_decimal = Decimal(f"-{'9'*12}.999999")  # Precision=18 and scale=6
max_18_6_decimal = Decimal(f"{'9'*12}.999999")  # Precision=18 and scale=6

actor_gln = "1234567890123"
actor_eic = "1234567890123456"


@pytest.mark.parametrize(
    "column_name,column_value",
    [
        (
            TotalMonthlyAmountsColumnNames.calculation_id,
            "9252d7a0-4363-42cc-a2d6-e04c026523f8",
        ),
        (TotalMonthlyAmountsColumnNames.calculation_type, "WholesaleFixing"),
        (
            TotalMonthlyAmountsColumnNames.calculation_result_id,
            "9252d7a0-4363-42cc-a2d6-e04c026523f8",
        ),
        (TotalMonthlyAmountsColumnNames.grid_area, "123"),
        (TotalMonthlyAmountsColumnNames.grid_area, "007"),
        (TotalMonthlyAmountsColumnNames.energy_supplier_id, actor_gln),
        (TotalMonthlyAmountsColumnNames.energy_supplier_id, actor_eic),
        (TotalMonthlyAmountsColumnNames.time, datetime(2020, 1, 1, 0, 0)),
        (TotalMonthlyAmountsColumnNames.amount, max_18_6_decimal),
        (TotalMonthlyAmountsColumnNames.amount, min_18_6_decimal),
        (TotalMonthlyAmountsColumnNames.charge_owner_id, actor_gln),
        (TotalMonthlyAmountsColumnNames.charge_owner_id, actor_eic),
        (TotalMonthlyAmountsColumnNames.charge_owner_id, None),
    ],
)
def test__migrated_table_accepts_valid_data(
    spark: SparkSession,
    column_name: str,
    column_value: str | list,
    migrations_executed: None,
) -> None:
    # Arrange
    result_df = _create_df(spark)
    result_df = set_column(result_df, column_name, column_value)

    # Act and assert: Expectation is that no exception is raised
    result_df.write.format("delta").option("mergeSchema", "false").insertInto(
        f"{OUTPUT_DATABASE_NAME}.{TOTAL_MONTHLY_AMOUNTS_TABLE_NAME}"
    )


@pytest.mark.parametrize(
    "column_name,column_value",
    [
        *[
            (TotalMonthlyAmountsColumnNames.calculation_type, x)
            for x in [
                CalculationType.WHOLESALE_FIXING.value,
                CalculationType.FIRST_CORRECTION_SETTLEMENT.value,
                CalculationType.SECOND_CORRECTION_SETTLEMENT.value,
                CalculationType.THIRD_CORRECTION_SETTLEMENT.value,
            ]
        ],
    ],
)
def test__migrated_table_accepts_enum_value(
    spark: SparkSession,
    column_name: str,
    column_value: str,
    migrations_executed: None,
) -> None:
    "Test that all enum values are accepted by the delta table"

    # Arrange
    result_df = _create_df(spark)
    result_df = set_column(result_df, column_name, column_value)

    # Act and assert: Expectation is that no exception is raised
    result_df.write.format("delta").option("mergeSchema", "false").insertInto(
        f"{OUTPUT_DATABASE_NAME}.{TOTAL_MONTHLY_AMOUNTS_TABLE_NAME}"
    )


@pytest.mark.parametrize(
    "amount",
    [
        min_18_6_decimal,
        max_18_6_decimal,
        Decimal("0.000000"),
        Decimal("0.000001"),
        Decimal("0.000005"),
        Decimal("0.000009"),
    ],
)
def test__migrated_table_does_not_round_valid_decimal(
    spark: SparkSession,
    amount: Decimal,
    migrations_executed: None,
) -> None:
    # Arrange
    result_df = _create_df(spark)
    result_df = result_df.withColumn("amount", lit(amount))
    calculation_id = str(uuid.uuid4())
    result_df = result_df.withColumn(
        TotalMonthlyAmountsColumnNames.calculation_id, lit(calculation_id)
    )

    # Act
    result_df.write.format("delta").option("mergeSchema", "false").insertInto(
        f"{OUTPUT_DATABASE_NAME}.{TOTAL_MONTHLY_AMOUNTS_TABLE_NAME}"
    )

    # Assert
    actual_df = spark.read.table(
        f"{OUTPUT_DATABASE_NAME}.{TOTAL_MONTHLY_AMOUNTS_TABLE_NAME}"
    ).where(col(TotalMonthlyAmountsColumnNames.calculation_id) == calculation_id)
    assert actual_df.collect()[0].amount == amount


def test__total_monthly_amounts_table__is_not_managed(
    spark: SparkSession, migrations_executed: None
) -> None:
    """
    It is desired that the table is unmanaged to provide for greater flexibility.
    According to https://learn.microsoft.com/en-us/azure/databricks/lakehouse/data-objects#--what-is-a-database:
    "To manage data life cycle independently of database, save data to a location that is not nested under any database locations."
    Thus we check whether the table is managed by comparing its location to the location of the database/schema.
    """
    database_details = spark.sql(f"DESCRIBE DATABASE {OUTPUT_DATABASE_NAME}")
    table_details = spark.sql(
        f"DESCRIBE DETAIL {OUTPUT_DATABASE_NAME}.{TOTAL_MONTHLY_AMOUNTS_TABLE_NAME}"
    )

    database_location = database_details.where(
        col("info_name") == "Location"
    ).collect()[0]["info_value"]
    table_location = table_details.collect()[0]["location"]

    assert not table_location.startswith(database_location)

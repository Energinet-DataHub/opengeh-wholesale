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
import uuid
from datetime import datetime
from decimal import Decimal

import pytest
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import col, lit

from package.calculation.output.schemas.monthly_amounts_schema import (
    monthly_amounts_schema,
)
from package.codelists import CalculationType, ChargeType
from package.constants import MonthlyAmountsColumnNames
from package.infrastructure.paths import WholesaleResultsInternalDatabase
from tests.helpers.data_frame_utils import set_column


def _create_df(spark: SparkSession) -> DataFrame:
    row = {
        MonthlyAmountsColumnNames.calculation_id: "9252d7a0-4363-42cc-a2d6-e04c026523f8",
        MonthlyAmountsColumnNames.calculation_type: "wholesale_fixing",
        MonthlyAmountsColumnNames.calculation_execution_time_start: datetime(
            2020, 1, 1, 0, 0
        ),
        MonthlyAmountsColumnNames.calculation_result_id: "6033ab5c-436b-44e9-8a79-90489d324e53",
        MonthlyAmountsColumnNames.grid_area_code: "543",
        MonthlyAmountsColumnNames.energy_supplier_id: "1234567890123",
        MonthlyAmountsColumnNames.quantity_unit: "kWh",
        MonthlyAmountsColumnNames.time: datetime(2020, 1, 1, 0, 0),
        MonthlyAmountsColumnNames.amount: Decimal("1.123"),
        MonthlyAmountsColumnNames.is_tax: True,
        MonthlyAmountsColumnNames.charge_code: "charge_code",
        MonthlyAmountsColumnNames.charge_type: ChargeType.SUBSCRIPTION.value,
        MonthlyAmountsColumnNames.charge_owner_id: "1234567890123",
    }
    return spark.createDataFrame(data=[row], schema=monthly_amounts_schema)


@pytest.mark.parametrize(
    "column_name,invalid_column_value",
    [
        (MonthlyAmountsColumnNames.calculation_id, None),
        (MonthlyAmountsColumnNames.calculation_id, "not-a-uuid"),
        (MonthlyAmountsColumnNames.calculation_type, None),
        (MonthlyAmountsColumnNames.calculation_type, "foo"),
        (MonthlyAmountsColumnNames.calculation_execution_time_start, None),
        (MonthlyAmountsColumnNames.calculation_result_id, None),
        (MonthlyAmountsColumnNames.calculation_result_id, "not-a-uuid"),
        (MonthlyAmountsColumnNames.grid_area_code, None),
        (MonthlyAmountsColumnNames.grid_area_code, "12"),
        (MonthlyAmountsColumnNames.grid_area_code, "1234"),
        (
            MonthlyAmountsColumnNames.energy_supplier_id,
            "neither-16-nor-13-digits-long",
        ),
        (MonthlyAmountsColumnNames.energy_supplier_id, None),
        (
            MonthlyAmountsColumnNames.quantity_unit,
            None,
        ),
        (
            MonthlyAmountsColumnNames.quantity_unit,
            "invalid",
        ),
        (MonthlyAmountsColumnNames.time, None),
        (
            MonthlyAmountsColumnNames.is_tax,
            None,
        ),
        (
            MonthlyAmountsColumnNames.charge_code,
            None,
        ),
        (
            MonthlyAmountsColumnNames.charge_type,
            "invalid",
        ),
        (
            MonthlyAmountsColumnNames.charge_type,
            None,
        ),
        (
            MonthlyAmountsColumnNames.charge_owner_id,
            "neither-16-nor-13-digits-long",
        ),
        (
            MonthlyAmountsColumnNames.charge_owner_id,
            None,
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
            f"{WholesaleResultsInternalDatabase.DATABASE_NAME}.{WholesaleResultsInternalDatabase.MONTHLY_AMOUNTS_PER_CHARGE_TABLE_NAME}",
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
            MonthlyAmountsColumnNames.calculation_id,
            "9252d7a0-4363-42cc-a2d6-e04c026523f8",
        ),
        (MonthlyAmountsColumnNames.calculation_type, "wholesale_fixing"),
        (
            MonthlyAmountsColumnNames.calculation_result_id,
            "9252d7a0-4363-42cc-a2d6-e04c026523f8",
        ),
        (MonthlyAmountsColumnNames.grid_area_code, "123"),
        (MonthlyAmountsColumnNames.grid_area_code, "007"),
        (MonthlyAmountsColumnNames.energy_supplier_id, actor_gln),
        (MonthlyAmountsColumnNames.energy_supplier_id, actor_eic),
        (MonthlyAmountsColumnNames.quantity_unit, "kWh"),
        (MonthlyAmountsColumnNames.quantity_unit, "pcs"),
        (MonthlyAmountsColumnNames.time, datetime(2020, 1, 1, 0, 0)),
        (MonthlyAmountsColumnNames.amount, max_18_6_decimal),
        (MonthlyAmountsColumnNames.amount, min_18_6_decimal),
        (MonthlyAmountsColumnNames.is_tax, True),
        (MonthlyAmountsColumnNames.is_tax, False),
        (MonthlyAmountsColumnNames.charge_code, "valid-charge-code"),
        (MonthlyAmountsColumnNames.charge_type, ChargeType.SUBSCRIPTION.value),
        (MonthlyAmountsColumnNames.charge_type, ChargeType.FEE.value),
        (MonthlyAmountsColumnNames.charge_type, ChargeType.TARIFF.value),
        (MonthlyAmountsColumnNames.charge_owner_id, actor_gln),
        (MonthlyAmountsColumnNames.charge_owner_id, actor_eic),
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
        f"{WholesaleResultsInternalDatabase.DATABASE_NAME}.{WholesaleResultsInternalDatabase.MONTHLY_AMOUNTS_PER_CHARGE_TABLE_NAME}"
    )


@pytest.mark.parametrize(
    "column_name,column_value",
    [
        *[
            (MonthlyAmountsColumnNames.calculation_type, x)
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
        f"{WholesaleResultsInternalDatabase.DATABASE_NAME}.{WholesaleResultsInternalDatabase.MONTHLY_AMOUNTS_PER_CHARGE_TABLE_NAME}"
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
        MonthlyAmountsColumnNames.calculation_id, lit(calculation_id)
    )

    # Act
    result_df.write.format("delta").option("mergeSchema", "false").insertInto(
        f"{WholesaleResultsInternalDatabase.DATABASE_NAME}.{WholesaleResultsInternalDatabase.MONTHLY_AMOUNTS_PER_CHARGE_TABLE_NAME}"
    )

    # Assert
    actual_df = spark.read.table(
        f"{WholesaleResultsInternalDatabase.DATABASE_NAME}.{WholesaleResultsInternalDatabase.MONTHLY_AMOUNTS_PER_CHARGE_TABLE_NAME}"
    ).where(col(MonthlyAmountsColumnNames.calculation_id) == calculation_id)
    assert actual_df.collect()[0].amount == amount

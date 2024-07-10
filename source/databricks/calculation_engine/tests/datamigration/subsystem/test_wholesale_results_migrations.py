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

from package.databases.table_column_names import TableColumnNames
from package.databases.wholesale_results_internal.schemas import (
    amounts_per_charge_schema,
)

from package.infrastructure.paths import WholesaleResultsInternalDatabase
from tests.helpers.data_frame_utils import set_column

TABLE_NAME = f"{WholesaleResultsInternalDatabase.DATABASE_NAME}.{WholesaleResultsInternalDatabase.AMOUNTS_PER_CHARGE_TABLE_NAME}"


def _create_df(spark: SparkSession) -> DataFrame:
    row = {
        TableColumnNames.calculation_id: "9252d7a0-4363-42cc-a2d6-e04c026523f8",
        TableColumnNames.result_id: "6033ab5c-436b-44e9-8a79-90489d324e53",
        TableColumnNames.grid_area_code: "543",
        TableColumnNames.energy_supplier_id: "1234567890123",
        TableColumnNames.quantity: Decimal("1.123"),
        TableColumnNames.quantity_qualities: ["missing"],
        TableColumnNames.time: datetime(2020, 1, 1, 0, 0),
        TableColumnNames.quantity_unit: "kWh",
        TableColumnNames.resolution: "P1D",
        TableColumnNames.metering_point_type: "production",
        TableColumnNames.settlement_method: "flex",
        TableColumnNames.price: Decimal("1.123"),
        TableColumnNames.amount: Decimal("1.123"),
        TableColumnNames.is_tax: True,
        TableColumnNames.charge_code: "charge_code",
        TableColumnNames.charge_type: "fee",
        TableColumnNames.charge_owner_id: "1234567890123",
    }
    return spark.createDataFrame(data=[row], schema=amounts_per_charge_schema)


@pytest.mark.parametrize(
    "column_name,invalid_column_value",
    [
        (TableColumnNames.calculation_id, None),
        (TableColumnNames.calculation_id, "not-a-uuid"),
        (TableColumnNames.result_id, None),
        (TableColumnNames.result_id, "not-a-uuid"),
        (TableColumnNames.grid_area_code, None),
        (TableColumnNames.grid_area_code, "12"),
        (TableColumnNames.grid_area_code, "1234"),
        (TableColumnNames.energy_supplier_id, None),
        (
            TableColumnNames.energy_supplier_id,
            "neither-16-nor-13-digits-long",
        ),
        (
            TableColumnNames.quantity,
            None,
        ),
        (TableColumnNames.quantity_unit, None),
        (TableColumnNames.quantity_unit, "foo"),
        (TableColumnNames.quantity_qualities, []),
        (TableColumnNames.quantity_qualities, ["foo"]),
        (TableColumnNames.time, None),
        (TableColumnNames.resolution, None),
        (TableColumnNames.resolution, "foo"),
        (TableColumnNames.metering_point_type, None),
        (TableColumnNames.metering_point_type, "foo"),
        (TableColumnNames.settlement_method, "foo"),
        (TableColumnNames.charge_owner_id, "neither-16-nor-13-digits-long"),
        (TableColumnNames.is_tax, None),
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
            TABLE_NAME,
            overwrite=False,
        )

    # Assert: Do sufficient assertions to be confident that the expected violation has been caught
    actual_error_message = str(ex.value)
    assert "DeltaInvariantViolationException" in actual_error_message
    assert column_name in actual_error_message


# According to SME there is no upper bounds limit from a business perspective.
# The chosen precision of 18 should however not cause any problems as the limit on time series
# is precision 6. Thus 1e9 time series points can be summed without any problem.
min_18_3_decimal = Decimal(f"-{'9'*15}.999")  # Precision=18 and scale=3
max_18_3_decimal = Decimal(f"{'9'*15}.999")  # Precision=18 and scale=3

min_18_6_decimal = Decimal(f"-{'9'*12}.999999")  # Precision=18 and scale=6
max_18_6_decimal = Decimal(f"{'9'*12}.999999")  # Precision=18 and scale=6

actor_gln = "1234567890123"
actor_eic = "1234567890123456"


@pytest.mark.parametrize(
    "column_name,column_value",
    [
        (
            TableColumnNames.calculation_id,
            "9252d7a0-4363-42cc-a2d6-e04c026523f8",
        ),
        (
            TableColumnNames.result_id,
            "9252d7a0-4363-42cc-a2d6-e04c026523f8",
        ),
        (TableColumnNames.grid_area_code, "123"),
        (TableColumnNames.grid_area_code, "007"),
        (TableColumnNames.energy_supplier_id, actor_gln),
        (TableColumnNames.energy_supplier_id, actor_eic),
        (TableColumnNames.quantity, max_18_3_decimal),
        (TableColumnNames.quantity, min_18_3_decimal),
        (TableColumnNames.quantity_unit, "kWh"),
        (TableColumnNames.quantity_qualities, ["missing", "estimated"]),
        (TableColumnNames.quantity_qualities, None),
        (TableColumnNames.time, datetime(2020, 1, 1, 0, 0)),
        (TableColumnNames.resolution, "P1D"),
        (TableColumnNames.metering_point_type, "consumption"),
        (TableColumnNames.settlement_method, None),
        (TableColumnNames.settlement_method, "flex"),
        (TableColumnNames.price, None),
        (TableColumnNames.price, max_18_6_decimal),
        (TableColumnNames.price, min_18_6_decimal),
        (TableColumnNames.amount, max_18_6_decimal),
        (TableColumnNames.amount, min_18_6_decimal),
        (TableColumnNames.charge_code, "any-string"),
        (TableColumnNames.charge_type, "fee"),
        (TableColumnNames.charge_owner_id, actor_gln),
        (TableColumnNames.charge_owner_id, actor_eic),
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
        TABLE_NAME
    )


@pytest.mark.parametrize(
    "quantity",
    [
        min_18_3_decimal,
        max_18_3_decimal,
        Decimal("0.000"),
        Decimal("0.001"),
        Decimal("0.005"),
        Decimal("0.009"),
    ],
)
def test__migrated_table_does_not_round_valid_decimal(
    spark: SparkSession,
    quantity: Decimal,
    migrations_executed: None,
) -> None:
    # Arrange
    result_df = _create_df(spark)
    result_df = result_df.withColumn("quantity", lit(quantity))
    calculation_id = str(uuid.uuid4())
    result_df = result_df.withColumn(
        TableColumnNames.calculation_id, lit(calculation_id)
    )

    # Act
    result_df.write.format("delta").option("mergeSchema", "false").insertInto(
        TABLE_NAME
    )

    # Assert
    actual_df = spark.read.table(TABLE_NAME).where(
        col(TableColumnNames.calculation_id) == calculation_id
    )
    assert actual_df.collect()[0].quantity == quantity

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

from package.calculation.output.output_table_column_names import OutputTableColumnNames
from package.calculation.output.results.schemas import (
    amounts_per_charge_schema,
)

from package.infrastructure.paths import WholesaleResultsInternalDatabase
from tests.helpers.data_frame_utils import set_column

TABLE_NAME = f"{WholesaleResultsInternalDatabase.DATABASE_NAME}.{WholesaleResultsInternalDatabase.AMOUNTS_PER_CHARGE_TABLE_NAME}"


def _create_df(spark: SparkSession) -> DataFrame:
    row = {
        OutputTableColumnNames.calculation_id: "9252d7a0-4363-42cc-a2d6-e04c026523f8",
        OutputTableColumnNames.calculation_type: "wholesale_fixing",
        OutputTableColumnNames.calculation_execution_time_start: datetime(
            2020, 1, 1, 0, 0
        ),
        OutputTableColumnNames.result_id: "6033ab5c-436b-44e9-8a79-90489d324e53",
        OutputTableColumnNames.grid_area_code: "543",
        OutputTableColumnNames.energy_supplier_id: "1234567890123",
        OutputTableColumnNames.quantity: Decimal("1.123"),
        OutputTableColumnNames.quantity_qualities: ["missing"],
        OutputTableColumnNames.time: datetime(2020, 1, 1, 0, 0),
        OutputTableColumnNames.quantity_unit: "kWh",
        OutputTableColumnNames.resolution: "P1D",
        OutputTableColumnNames.metering_point_type: "production",
        OutputTableColumnNames.settlement_method: "flex",
        OutputTableColumnNames.price: Decimal("1.123"),
        OutputTableColumnNames.amount: Decimal("1.123"),
        OutputTableColumnNames.is_tax: True,
        OutputTableColumnNames.charge_code: "charge_code",
        OutputTableColumnNames.charge_type: "fee",
        OutputTableColumnNames.charge_owner_id: "1234567890123",
    }
    return spark.createDataFrame(data=[row], schema=amounts_per_charge_schema)


@pytest.mark.parametrize(
    "column_name,invalid_column_value",
    [
        (OutputTableColumnNames.calculation_id, None),
        (OutputTableColumnNames.calculation_id, "not-a-uuid"),
        (OutputTableColumnNames.result_id, None),
        (OutputTableColumnNames.result_id, "not-a-uuid"),
        (OutputTableColumnNames.grid_area_code, None),
        (OutputTableColumnNames.grid_area_code, "12"),
        (OutputTableColumnNames.grid_area_code, "1234"),
        (OutputTableColumnNames.energy_supplier_id, None),
        (
            OutputTableColumnNames.energy_supplier_id,
            "neither-16-nor-13-digits-long",
        ),
        (OutputTableColumnNames.quantity_unit, None),
        (OutputTableColumnNames.quantity_unit, "foo"),
        (OutputTableColumnNames.quantity_qualities, []),
        (OutputTableColumnNames.quantity_qualities, ["foo"]),
        (OutputTableColumnNames.time, None),
        (OutputTableColumnNames.resolution, None),
        (OutputTableColumnNames.resolution, "foo"),
        (OutputTableColumnNames.metering_point_type, None),
        (OutputTableColumnNames.metering_point_type, "foo"),
        (OutputTableColumnNames.settlement_method, "foo"),
        (OutputTableColumnNames.charge_owner_id, "neither-16-nor-13-digits-long"),
        (OutputTableColumnNames.is_tax, None),
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
            OutputTableColumnNames.calculation_id,
            "9252d7a0-4363-42cc-a2d6-e04c026523f8",
        ),
        (OutputTableColumnNames.calculation_type, "wholesale_fixing"),
        (
            OutputTableColumnNames.result_id,
            "9252d7a0-4363-42cc-a2d6-e04c026523f8",
        ),
        (OutputTableColumnNames.grid_area_code, "123"),
        (OutputTableColumnNames.grid_area_code, "007"),
        (OutputTableColumnNames.energy_supplier_id, actor_gln),
        (OutputTableColumnNames.energy_supplier_id, actor_eic),
        (OutputTableColumnNames.quantity, max_18_3_decimal),
        (OutputTableColumnNames.quantity, min_18_3_decimal),
        (OutputTableColumnNames.quantity_unit, "kWh"),
        (OutputTableColumnNames.quantity_qualities, ["missing", "estimated"]),
        (OutputTableColumnNames.quantity_qualities, None),
        (OutputTableColumnNames.time, datetime(2020, 1, 1, 0, 0)),
        (OutputTableColumnNames.resolution, "P1D"),
        (OutputTableColumnNames.metering_point_type, "consumption"),
        (OutputTableColumnNames.settlement_method, None),
        (OutputTableColumnNames.settlement_method, "flex"),
        (OutputTableColumnNames.price, None),
        (OutputTableColumnNames.price, max_18_6_decimal),
        (OutputTableColumnNames.price, min_18_6_decimal),
        (OutputTableColumnNames.amount, max_18_6_decimal),
        (OutputTableColumnNames.amount, min_18_6_decimal),
        (OutputTableColumnNames.charge_code, "any-string"),
        (OutputTableColumnNames.charge_type, "fee"),
        (OutputTableColumnNames.charge_owner_id, actor_gln),
        (OutputTableColumnNames.charge_owner_id, actor_eic),
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
        OutputTableColumnNames.calculation_id, lit(calculation_id)
    )

    # Act
    result_df.write.format("delta").option("mergeSchema", "false").insertInto(
        TABLE_NAME
    )

    # Assert
    actual_df = spark.read.table(TABLE_NAME).where(
        col(OutputTableColumnNames.calculation_id) == calculation_id
    )
    assert actual_df.collect()[0].quantity == quantity

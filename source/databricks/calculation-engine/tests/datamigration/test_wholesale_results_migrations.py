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
from typing import Union
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import array, col, lit
import pytest
import uuid

from package.codelists import (
    ChargeQuality,
    ChargeResolution,
    ChargeType,
    ChargeUnit,
    MeteringPointType,
    ProcessType,
    SettlementMethod,
)
from package.constants import WholesaleResultColumnNames
from package.infrastructure.paths import (
    OUTPUT_DATABASE_NAME,
    WHOLESALE_RESULT_TABLE_NAME,
)
from package.calculation_output.schemas import wholesale_results_schema


def _create_df(spark: SparkSession) -> DataFrame:
    row = {
        WholesaleResultColumnNames.calculation_id: "9252d7a0-4363-42cc-a2d6-e04c026523f8",
        WholesaleResultColumnNames.calculation_type: "WholesaleFixing",
        WholesaleResultColumnNames.calculation_execution_time_start: datetime(
            2020, 1, 1, 0, 0
        ),
        WholesaleResultColumnNames.calculation_result_id: "6033ab5c-436b-44e9-8a79-90489d324e53",
        WholesaleResultColumnNames.grid_area: "543",
        WholesaleResultColumnNames.energy_supplier_id: "1234567890123",
        WholesaleResultColumnNames.quantity: Decimal("1.123"),
        WholesaleResultColumnNames.quantity_qualities: ["missing"],
        WholesaleResultColumnNames.time: datetime(2020, 1, 1, 0, 0),
        WholesaleResultColumnNames.quantity_unit: "kWh",
        WholesaleResultColumnNames.resolution: "P1D",
        WholesaleResultColumnNames.metering_point_type: "production",
        WholesaleResultColumnNames.settlement_method: "flex",
        WholesaleResultColumnNames.price: Decimal("1.123"),
        WholesaleResultColumnNames.amount: Decimal("1.123"),
        WholesaleResultColumnNames.is_tax: True,
        WholesaleResultColumnNames.charge_code: "charge_code",
        WholesaleResultColumnNames.charge_type: "fee",
        WholesaleResultColumnNames.charge_owner_id: "1234567890123",
    }
    return spark.createDataFrame(data=[row], schema=wholesale_results_schema)


@pytest.mark.parametrize(
    "column_name,invalid_column_value",
    [
        (WholesaleResultColumnNames.calculation_id, None),
        (WholesaleResultColumnNames.calculation_id, "not-a-uuid"),
        (WholesaleResultColumnNames.calculation_type, None),
        (WholesaleResultColumnNames.calculation_type, "foo"),
        (WholesaleResultColumnNames.calculation_execution_time_start, None),
        (WholesaleResultColumnNames.calculation_result_id, None),
        (WholesaleResultColumnNames.calculation_result_id, "not-a-uuid"),
        (WholesaleResultColumnNames.grid_area, None),
        (WholesaleResultColumnNames.grid_area, "12"),
        (WholesaleResultColumnNames.grid_area, "1234"),
        (WholesaleResultColumnNames.energy_supplier_id, None),
        (
            WholesaleResultColumnNames.energy_supplier_id,
            "neither-16-nor-13-digits-long",
        ),
        (WholesaleResultColumnNames.quantity_unit, None),
        (WholesaleResultColumnNames.quantity_unit, "foo"),
        (WholesaleResultColumnNames.quantity_qualities, None),
        (WholesaleResultColumnNames.quantity_qualities, []),
        (WholesaleResultColumnNames.quantity_qualities, ["foo"]),
        (WholesaleResultColumnNames.time, None),
        (WholesaleResultColumnNames.resolution, None),
        (WholesaleResultColumnNames.resolution, "foo"),
        (WholesaleResultColumnNames.metering_point_type, "foo"),
        (WholesaleResultColumnNames.settlement_method, "foo"),
        (WholesaleResultColumnNames.charge_owner_id, "neither-16-nor-13-digits-long"),
    ],
)
def test__migrated_table_rejects_invalid_data(
    spark: SparkSession,
    column_name: str,
    invalid_column_value: Union[str, list],
    migrations_executed: None,
) -> None:
    # Arrange
    results_df = _create_df(spark)

    if isinstance(invalid_column_value, list):
        invalid_df = results_df.withColumn(
            column_name, array(*map(lit, invalid_column_value))
        )
    else:
        invalid_df = results_df.withColumn(column_name, lit(invalid_column_value))

    # Act
    with pytest.raises(Exception) as ex:
        invalid_df.write.format("delta").option("mergeSchema", "false").insertInto(
            f"{OUTPUT_DATABASE_NAME}.{WHOLESALE_RESULT_TABLE_NAME}", overwrite=False
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
            WholesaleResultColumnNames.calculation_id,
            "9252d7a0-4363-42cc-a2d6-e04c026523f8",
        ),
        (WholesaleResultColumnNames.calculation_type, "WholesaleFixing"),
        (
            WholesaleResultColumnNames.calculation_result_id,
            "9252d7a0-4363-42cc-a2d6-e04c026523f8",
        ),
        (WholesaleResultColumnNames.grid_area, "123"),
        (WholesaleResultColumnNames.grid_area, "007"),
        (WholesaleResultColumnNames.energy_supplier_id, None),
        (WholesaleResultColumnNames.energy_supplier_id, actor_gln),
        (WholesaleResultColumnNames.energy_supplier_id, actor_eic),
        (WholesaleResultColumnNames.quantity, None),
        (WholesaleResultColumnNames.quantity, max_18_3_decimal),
        (WholesaleResultColumnNames.quantity, min_18_3_decimal),
        (WholesaleResultColumnNames.quantity_unit, "kWh"),
        (WholesaleResultColumnNames.quantity_qualities, ["missing", "estimated"]),
        (WholesaleResultColumnNames.time, datetime(2020, 1, 1, 0, 0)),
        (WholesaleResultColumnNames.resolution, "P1D"),
        (WholesaleResultColumnNames.metering_point_type, None),
        (WholesaleResultColumnNames.metering_point_type, "consumption"),
        (WholesaleResultColumnNames.settlement_method, None),
        (WholesaleResultColumnNames.settlement_method, "flex"),
        (WholesaleResultColumnNames.price, None),
        (WholesaleResultColumnNames.price, max_18_6_decimal),
        (WholesaleResultColumnNames.price, min_18_6_decimal),
        (WholesaleResultColumnNames.amount, max_18_6_decimal),
        (WholesaleResultColumnNames.amount, min_18_6_decimal),
        (WholesaleResultColumnNames.is_tax, None),
        (WholesaleResultColumnNames.charge_code, "any-string"),
        (WholesaleResultColumnNames.charge_type, "fee"),
        (WholesaleResultColumnNames.charge_owner_id, actor_gln),
        (WholesaleResultColumnNames.charge_owner_id, actor_eic),
    ],
)
def test__migrated_table_accepts_valid_data(
    spark: SparkSession,
    column_name: str,
    column_value: Union[str, list],
    migrations_executed: None,
) -> None:
    # Arrange
    result_df = _create_df(spark)

    if isinstance(column_value, list):
        result_df = result_df.withColumn(column_name, array(*map(lit, column_value)))
    else:
        result_df = result_df.withColumn(column_name, lit(column_value))

    # Act and assert: Expectation is that no exception is raised
    result_df.write.format("delta").option("mergeSchema", "false").insertInto(
        f"{OUTPUT_DATABASE_NAME}.{WHOLESALE_RESULT_TABLE_NAME}"
    )


@pytest.mark.parametrize(
    "column_name,column_value",
    [
        *[
            (WholesaleResultColumnNames.calculation_type, x)
            for x in [
                ProcessType.WHOLESALE_FIXING.value,
                ProcessType.FIRST_CORRECTION_SETTLEMENT.value,
                ProcessType.SECOND_CORRECTION_SETTLEMENT.value,
                ProcessType.THIRD_CORRECTION_SETTLEMENT.value,
            ]
        ],
        *[(WholesaleResultColumnNames.quantity_unit, x.value) for x in ChargeUnit],
        *[
            (WholesaleResultColumnNames.quantity_qualities, [x.value])
            for x in ChargeQuality
        ],
        *[(WholesaleResultColumnNames.resolution, x.value) for x in ChargeResolution],
        *[
            (WholesaleResultColumnNames.metering_point_type, x.value)
            for x in MeteringPointType
        ],
        *[
            (WholesaleResultColumnNames.settlement_method, x.value)
            for x in SettlementMethod
        ],
        *[(WholesaleResultColumnNames.charge_type, x.value) for x in ChargeType],
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

    if isinstance(column_value, list):
        result_df = result_df.withColumn(column_name, array(*map(lit, column_value)))
    else:
        result_df = result_df.withColumn(column_name, lit(column_value))

    # Act and assert: Expectation is that no exception is raised
    result_df.write.format("delta").option("mergeSchema", "false").insertInto(
        f"{OUTPUT_DATABASE_NAME}.{WHOLESALE_RESULT_TABLE_NAME}"
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
        WholesaleResultColumnNames.calculation_id, lit(calculation_id)
    )

    # Act
    result_df.write.format("delta").option("mergeSchema", "false").insertInto(
        f"{OUTPUT_DATABASE_NAME}.{WHOLESALE_RESULT_TABLE_NAME}"
    )

    # Assert
    actual_df = spark.read.table(
        f"{OUTPUT_DATABASE_NAME}.{WHOLESALE_RESULT_TABLE_NAME}"
    ).where(col(WholesaleResultColumnNames.calculation_id) == calculation_id)
    assert actual_df.collect()[0].quantity == quantity


def test__wholesale_results_table__is_not_managed(
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
        f"DESCRIBE DETAIL {OUTPUT_DATABASE_NAME}.{WHOLESALE_RESULT_TABLE_NAME}"
    )

    database_location = database_details.where(
        col("info_name") == "Location"
    ).collect()[0]["info_value"]
    table_location = table_details.collect()[0]["location"]

    assert not table_location.startswith(database_location)

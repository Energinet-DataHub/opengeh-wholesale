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
from pyspark.sql.functions import lit, col

from helpers.data_frame_utils import set_column
from package.calculation.output.schemas import energy_results_schema
from package.codelists import (
    AggregationLevel,
    CalculationType,
    TimeSeriesType,
    QuantityQuality,
    MeteringPointResolution,
)
from package.constants import EnergyResultColumnNames
from package.infrastructure.paths import HiveOutputDatabase


def _create_df(spark: SparkSession) -> DataFrame:
    row = {
        EnergyResultColumnNames.grid_area_code: "543",
        EnergyResultColumnNames.energy_supplier_id: "1234567890123",
        EnergyResultColumnNames.balance_responsible_id: "9876543210987",
        EnergyResultColumnNames.quantity: Decimal("1.123"),
        EnergyResultColumnNames.quantity_qualities: ["missing"],
        EnergyResultColumnNames.time: datetime(2020, 1, 1, 0, 0),
        EnergyResultColumnNames.aggregation_level: "total_ga",
        EnergyResultColumnNames.time_series_type: "production",
        EnergyResultColumnNames.calculation_id: "9252d7a0-4363-42cc-a2d6-e04c026523f8",
        EnergyResultColumnNames.calculation_type: "balance_fixing",
        EnergyResultColumnNames.calculation_execution_time_start: datetime(
            2020, 1, 1, 0, 0
        ),
        EnergyResultColumnNames.neighbor_grid_area_code: "843",
        EnergyResultColumnNames.calculation_result_id: "6033ab5c-436b-44e9-8a79-90489d324e53",
        EnergyResultColumnNames.metering_point_id: None,
        EnergyResultColumnNames.resolution: MeteringPointResolution.QUARTER.value,
    }
    return spark.createDataFrame(data=[row], schema=energy_results_schema)


@pytest.mark.parametrize(
    "column_name,invalid_column_value",
    [
        (EnergyResultColumnNames.calculation_id, None),
        (EnergyResultColumnNames.calculation_execution_time_start, None),
        (EnergyResultColumnNames.calculation_type, None),
        (EnergyResultColumnNames.calculation_type, "foo"),
        (EnergyResultColumnNames.time_series_type, None),
        (EnergyResultColumnNames.time_series_type, "foo"),
        (EnergyResultColumnNames.grid_area_code, None),
        (EnergyResultColumnNames.grid_area_code, "12"),
        (EnergyResultColumnNames.grid_area_code, "1234"),
        (EnergyResultColumnNames.neighbor_grid_area_code, "12"),
        (EnergyResultColumnNames.neighbor_grid_area_code, "1234"),
        (EnergyResultColumnNames.time, None),
        (EnergyResultColumnNames.quantity, None),
        (EnergyResultColumnNames.quantity_qualities, []),
        (EnergyResultColumnNames.quantity_qualities, [None]),
        (EnergyResultColumnNames.quantity_qualities, ["foo"]),
        (EnergyResultColumnNames.quantity_qualities, ["incomplete"]),
        (EnergyResultColumnNames.aggregation_level, None),
        (EnergyResultColumnNames.aggregation_level, "foo"),
        (EnergyResultColumnNames.resolution, None),
        (EnergyResultColumnNames.resolution, "foo"),
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
            f"{HiveOutputDatabase.DATABASE_NAME}.{HiveOutputDatabase.ENERGY_RESULT_TABLE_NAME}",
            overwrite=False,
        )

    # Assert: Do sufficient assertions to be confident that the expected violation has been caught
    actual_error_message = str(ex.value)
    assert "DeltaInvariantViolationException" in actual_error_message
    assert column_name in actual_error_message


# According to SME there is no upper bounds limit from a business perspective.
# The chosen precision of 18 should however not cause any problems as the limit on time series
# is precision 6. Thus 1e9 time series points can be summed without any problem.
min_decimal = Decimal(f"-{'9'*15}.999")  # Precision=18 and scale=3
max_decimal = Decimal(f"{'9'*15}.999")  # Precision=18 and scale=3


@pytest.mark.parametrize(
    "column_name,column_value",
    [
        (
            EnergyResultColumnNames.calculation_id,
            "9252d7a0-4363-42cc-a2d6-e04c026523f8",
        ),
        (EnergyResultColumnNames.grid_area_code, "123"),
        (EnergyResultColumnNames.grid_area_code, "007"),
        (EnergyResultColumnNames.neighbor_grid_area_code, None),
        (EnergyResultColumnNames.neighbor_grid_area_code, "123"),
        (EnergyResultColumnNames.neighbor_grid_area_code, "007"),
        (EnergyResultColumnNames.balance_responsible_id, None),
        (EnergyResultColumnNames.balance_responsible_id, "1234567890123"),
        (EnergyResultColumnNames.energy_supplier_id, None),
        (EnergyResultColumnNames.energy_supplier_id, "1234567890123"),
        (EnergyResultColumnNames.quantity, Decimal("1.123")),
        (EnergyResultColumnNames.quantity, max_decimal),
        (EnergyResultColumnNames.quantity, -max_decimal),
        (EnergyResultColumnNames.quantity_qualities, ["estimated"]),
        (EnergyResultColumnNames.quantity_qualities, ["estimated", "measured"]),
    ],
)
def test__migrated_table_accepts_valid_data(
    spark: SparkSession,
    column_name: str,
    column_value: str | list,
) -> None:
    # Arrange
    result_df = _create_df(spark)
    result_df = set_column(result_df, column_name, column_value)

    # Act and assert: Expectation is that no exception is raised
    result_df.write.format("delta").option("mergeSchema", "false").insertInto(
        f"{HiveOutputDatabase.DATABASE_NAME}.{HiveOutputDatabase.ENERGY_RESULT_TABLE_NAME}"
    )


@pytest.mark.parametrize(
    "column_name,column_value",
    [
        *[(EnergyResultColumnNames.calculation_type, x.value) for x in CalculationType],
        *[(EnergyResultColumnNames.time_series_type, x.value) for x in TimeSeriesType],
        *[
            (EnergyResultColumnNames.quantity_qualities, [x.value])
            for x in QuantityQuality
        ],
        *[
            (EnergyResultColumnNames.aggregation_level, x.value)
            for x in AggregationLevel
        ],
        *[
            (EnergyResultColumnNames.resolution, x.value)
            for x in MeteringPointResolution
        ],
    ],
)
def test__migrated_table_accepts_enum_value(
    spark: SparkSession,
    column_name: str,
    column_value: str | list,
    migrations_executed: None,
) -> None:
    """Test that all enum values are accepted by the delta table"""

    # Arrange
    result_df = _create_df(spark)
    result_df = set_column(result_df, column_name, column_value)
    # this is a special case where we need to set the metering point id to a valid value
    # do to constraints on the table seen in 202401161030_Add_metering_point_id_to_energy_table.sql
    if column_value == "negative_grid_loss" or column_value == "positive_grid_loss":
        result_df = set_column(
            result_df, EnergyResultColumnNames.metering_point_id, "571313180480500149"
        )

    # Act and assert: Expectation is that no exception is raised
    result_df.write.format("delta").option("mergeSchema", "false").insertInto(
        f"{HiveOutputDatabase.DATABASE_NAME}.{HiveOutputDatabase.ENERGY_RESULT_TABLE_NAME}"
    )


@pytest.mark.parametrize(
    "quantity",
    [
        min_decimal,
        max_decimal,
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
        EnergyResultColumnNames.calculation_id, lit(calculation_id)
    )

    # Act
    result_df.write.format("delta").option("mergeSchema", "false").insertInto(
        f"{HiveOutputDatabase.DATABASE_NAME}.{HiveOutputDatabase.ENERGY_RESULT_TABLE_NAME}"
    )

    # Assert
    actual_df = spark.read.table(
        f"{HiveOutputDatabase.DATABASE_NAME}.{HiveOutputDatabase.ENERGY_RESULT_TABLE_NAME}"
    ).where(col(EnergyResultColumnNames.calculation_id) == calculation_id)
    assert actual_df.collect()[0].quantity == quantity


def test__result_table__is_not_managed(
    spark: SparkSession, migrations_executed: None
) -> None:
    """
    It is desired that the table is unmanaged to provide for greater flexibility.
    According to https://learn.microsoft.com/en-us/azure/databricks/lakehouse/data-objects#--what-is-a-database:
    "To manage data life cycle independently of database, save data to a location that is not nested under any database locations."
    Thus we check whether the table is managed by comparing its location to the location of the database/schema.
    """
    database_details = spark.sql(
        f"DESCRIBE DATABASE {HiveOutputDatabase.DATABASE_NAME}"
    )
    table_details = spark.sql(
        f"DESCRIBE DETAIL {HiveOutputDatabase.DATABASE_NAME}.{HiveOutputDatabase.ENERGY_RESULT_TABLE_NAME}"
    )

    database_location = database_details.where(
        col("info_name") == "Location"
    ).collect()[0]["info_value"]
    table_location = table_details.collect()[0]["location"]

    assert not table_location.startswith(database_location)


@pytest.mark.parametrize(
    "metering_point_id,time_series_type",
    [
        (None, TimeSeriesType.PRODUCTION.value),
        (None, TimeSeriesType.NON_PROFILED_CONSUMPTION.value),
        (None, TimeSeriesType.NET_EXCHANGE_PER_NEIGHBORING_GA.value),
        (None, TimeSeriesType.NET_EXCHANGE_PER_GA.value),
        (None, TimeSeriesType.FLEX_CONSUMPTION.value),
        (None, TimeSeriesType.GRID_LOSS.value),
        ("571313180480500149", TimeSeriesType.NEGATIVE_GRID_LOSS.value),
        ("571313180480500149", TimeSeriesType.POSITIVE_GRID_LOSS.value),
        (None, TimeSeriesType.TOTAL_CONSUMPTION.value),
        (None, TimeSeriesType.TEMP_FLEX_CONSUMPTION.value),
        (None, TimeSeriesType.TEMP_PRODUCTION.value),
    ],
)
def test__migrated_table_constraints_on_metering_point_id_with_valid_data(
    spark: SparkSession,
    metering_point_id: str | None,
    time_series_type: str,
    migrations_executed: None,
) -> None:
    # Arrange
    results_df = _create_df(spark)

    results_df = set_column(
        results_df, EnergyResultColumnNames.time_series_type, time_series_type
    )
    results_df = set_column(
        results_df, EnergyResultColumnNames.metering_point_id, metering_point_id
    )

    # Act + Assert
    results_df.write.format("delta").option("mergeSchema", "false").insertInto(
        f"{HiveOutputDatabase.DATABASE_NAME}.{HiveOutputDatabase.ENERGY_RESULT_TABLE_NAME}"
    )


@pytest.mark.parametrize(
    "metering_point_id,time_series_type",
    [
        ("571313180480500149", TimeSeriesType.PRODUCTION.value),
        ("571313180480500149", TimeSeriesType.NON_PROFILED_CONSUMPTION.value),
        ("571313180480500149", TimeSeriesType.NET_EXCHANGE_PER_NEIGHBORING_GA.value),
        ("571313180480500149", TimeSeriesType.NET_EXCHANGE_PER_GA.value),
        ("571313180480500149", TimeSeriesType.FLEX_CONSUMPTION.value),
        ("571313180480500149", TimeSeriesType.GRID_LOSS.value),
        (None, TimeSeriesType.NEGATIVE_GRID_LOSS.value),
        (None, TimeSeriesType.POSITIVE_GRID_LOSS.value),
        ("not_18_length", TimeSeriesType.NEGATIVE_GRID_LOSS.value),
        ("not_18_length", TimeSeriesType.POSITIVE_GRID_LOSS.value),
        ("571313180480500149", TimeSeriesType.TOTAL_CONSUMPTION.value),
        ("571313180480500149", TimeSeriesType.TEMP_FLEX_CONSUMPTION.value),
        ("571313180480500149", TimeSeriesType.TEMP_PRODUCTION.value),
    ],
)
def test__migrated_table_constraints_on_metering_point_id_with_invalid_data(
    spark: SparkSession,
    metering_point_id: str | None,
    time_series_type: str,
    migrations_executed: None,
) -> None:
    # Arrange
    results_df = _create_df(spark)

    results_df = set_column(
        results_df, EnergyResultColumnNames.time_series_type, time_series_type
    )
    results_df = set_column(
        results_df, EnergyResultColumnNames.metering_point_id, metering_point_id
    )

    # Act
    with pytest.raises(Exception) as ex:
        results_df.write.format("delta").option("mergeSchema", "false").insertInto(
            f"{HiveOutputDatabase.DATABASE_NAME}.{HiveOutputDatabase.ENERGY_RESULT_TABLE_NAME}",
            overwrite=False,
        )

    # Assert: Do sufficient assertions to be confident that the expected violation has been caught
    actual_error_message = str(ex.value)
    assert "DeltaInvariantViolationException" in actual_error_message
    assert EnergyResultColumnNames.metering_point_id in actual_error_message

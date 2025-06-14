from pyspark.sql import SparkSession
from pyspark.sql.functions import StructType

from geh_wholesale.databases.migrations_wholesale.schemas import (
    charge_link_periods_schema,
    charge_price_information_periods_schema,
    charge_price_points_schema,
    metering_point_periods_schema,
    time_series_points_schema,
)
from geh_wholesale.infrastructure import paths


def test__input_time_series_point_schema__matches_published_contract(
    spark: SparkSession,
    energy_input_data_written_to_delta: None,
    calculation_input_database: str,
) -> None:
    # Act: Calculator job is executed just once per session. See the fixture `executed_balance_fixing`

    # Assert
    actual_input_data = spark.read.table(
        f"{calculation_input_database}.{paths.MigrationsWholesaleDatabase.TIME_SERIES_POINTS_TABLE_NAME}"
    )

    # When asserting both that the calculator creates output, and it does it with input data that matches
    # the time series points contract from the time-series subsystem (in the same test), then we can infer that the
    # calculator works with the format of the data published from the time-series subsystem.
    # NOTE:It is not evident from this test that it uses the same input as the calculator job
    # Apparently nullability is ignored for CSV sources, so we have to compare schemas in this slightly odd way
    # See more at https://stackoverflow.com/questions/50609548/compare-schema-ignoring-nullable
    _assert_is_equal(actual_input_data.schema, time_series_points_schema)


def test__input_metering_point_period_schema__matches_published_contract(
    spark: SparkSession,
    energy_input_data_written_to_delta: None,
    calculation_input_database: str,
) -> None:
    # Assert
    test_input_data = spark.read.table(
        f"{calculation_input_database}.{paths.MigrationsWholesaleDatabase.METERING_POINT_PERIODS_TABLE_NAME}"
    )
    _assert_is_equal(test_input_data.schema, metering_point_periods_schema)


def test__input_charge_link_period_schema__matches_published_contract(
    spark: SparkSession,
    price_input_data_written_to_delta: None,
    calculation_input_database: str,
) -> None:
    # Assert
    test_input_data = spark.read.table(
        f"{calculation_input_database}.{paths.MigrationsWholesaleDatabase.CHARGE_LINK_PERIODS_TABLE_NAME}"
    )
    _assert_is_equal(test_input_data.schema, charge_link_periods_schema)


def test__input_charge_price_points_schema__matches_published_contract(
    spark: SparkSession,
    price_input_data_written_to_delta: None,
    calculation_input_database: str,
) -> None:
    # Assert
    test_input_data = spark.read.table(
        f"{calculation_input_database}.{paths.MigrationsWholesaleDatabase.CHARGE_PRICE_POINTS_TABLE_NAME}"
    )
    _assert_is_equal(test_input_data.schema, charge_price_points_schema)


def test__input_charge_price_information_periods_schema__matches_published_contract(
    spark: SparkSession,
    price_input_data_written_to_delta: None,
    calculation_input_database: str,
) -> None:
    # Assert
    test_input_data = spark.read.table(
        f"{calculation_input_database}.{paths.MigrationsWholesaleDatabase.CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME}"
    )
    _assert_is_equal(test_input_data.schema, charge_price_information_periods_schema)


def _assert_is_equal(actual_schema: StructType, expected_schema: StructType) -> None:
    assert all((a.name, a.dataType) == (b.name, b.dataType) for a, b in zip(actual_schema, expected_schema))

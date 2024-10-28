import copy

from pyspark.sql import SparkSession
import pytest

from tests.dbutils_fixture import DBUtilsFixture

from data_seeding import (
    standard_wholesale_fixing_scenario_data_generator,
    standard_balance_fixing_scenario_data_generator,
)
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.report_generator import execute_quarterly_time_series
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.domain.csv_column_names import (
    CsvColumnNames,
)


@pytest.fixture(scope="function", autouse=True)
def reset_task_values(dbutils: DBUtilsFixture):
    yield
    print("Resetting task values")
    dbutils.jobs.taskValues.reset()


def test_execute_quarterly_time_series__when_energy_supplier__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    args = copy.deepcopy(standard_wholesale_fixing_scenario_args)
    args.requesting_actor_market_role = MarketRole.ENERGY_SUPPLIER
    energy_supplier_id = (
        standard_wholesale_fixing_scenario_data_generator.ENERGY_SUPPLIER_IDS[0]
    )
    args.requesting_actor_id = energy_supplier_id
    args.energy_supplier_ids = [energy_supplier_id]
    expected_file_names = [
        f"TSSD15_804_{energy_supplier_id}_DDQ_02-01-2024_02-01-2024.csv",
        f"TSSD15_805_{energy_supplier_id}_DDQ_02-01-2024_02-01-2024.csv",
    ]
    expected_columns = [
        CsvColumnNames.metering_point_id,
        CsvColumnNames.type_of_mp,
        CsvColumnNames.start_date_time,
    ] + [f"ENERGYQUANTITY{i}" for i in range(1, 101)]

    # Act
    execute_quarterly_time_series(spark, dbutils, args)

    # Assert
    actual_files = dbutils.jobs.taskValues.get(key="quarterly_time_series_files")
    assert len(actual_files) == len(expected_file_names)
    for file_path in actual_files:
        df = spark.read.option("delimiter", ";").csv(file_path, header=True)
        assert df.count() > 0
        assert df.columns == expected_columns
        assert any(file_name in file_path for file_name in expected_file_names)


def test_execute_quarterly_time_series__when_grid_access_provider__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    args = copy.deepcopy(standard_wholesale_fixing_scenario_args)
    args.requesting_actor_market_role = MarketRole.GRID_ACCESS_PROVIDER
    args.energy_supplier_ids = None
    expected_file_names = [
        f"TSSD15_804_{args.requesting_actor_id}_DDM_02-01-2024_02-01-2024.csv",
        f"TSSD15_805_{args.requesting_actor_id}_DDM_02-01-2024_02-01-2024.csv",
    ]
    expected_columns = [
        CsvColumnNames.metering_point_id,
        CsvColumnNames.type_of_mp,
        CsvColumnNames.start_date_time,
    ] + [f"ENERGYQUANTITY{i}" for i in range(1, 101)]

    # Act
    execute_quarterly_time_series(spark, dbutils, args)

    # Assert
    actual_files = dbutils.jobs.taskValues.get("quarterly_time_series_files")
    assert len(actual_files) == len(expected_file_names)
    for file_path in actual_files:
        df = spark.read.option("delimiter", ";").csv(file_path, header=True)
        assert df.count() > 0
        assert df.columns == expected_columns
        assert any(file_name in file_path for file_name in expected_file_names)


@pytest.mark.parametrize(
    "market_role",
    [MarketRole.SYSTEM_OPERATOR, MarketRole.DATAHUB_ADMINISTRATOR],
)
def test_execute_quarterly_time_series__when_system_operator_or_datahub_admin_with_one_energy_supplier_id__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
    market_role: MarketRole,
):
    # Arrange
    args = copy.deepcopy(standard_wholesale_fixing_scenario_args)
    args.requesting_actor_market_role = market_role
    energy_supplier_id = (
        standard_wholesale_fixing_scenario_data_generator.ENERGY_SUPPLIER_IDS[0]
    )
    args.energy_supplier_ids = [energy_supplier_id]
    expected_file_names = [
        f"TSSD15_804_{energy_supplier_id}_02-01-2024_02-01-2024.csv",
        f"TSSD15_805_{energy_supplier_id}_02-01-2024_02-01-2024.csv",
    ]
    expected_columns = [
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.metering_point_id,
        CsvColumnNames.type_of_mp,
        CsvColumnNames.start_date_time,
    ] + [f"ENERGYQUANTITY{i}" for i in range(1, 101)]

    # Act
    execute_quarterly_time_series(spark, dbutils, args)

    # Assert
    actual_files = dbutils.jobs.taskValues.get("quarterly_time_series_files")
    assert len(actual_files) == len(expected_file_names)
    for file_path in actual_files:
        df = spark.read.option("delimiter", ";").csv(file_path, header=True)
        assert df.count() > 0
        assert df.columns == expected_columns
        assert any(file_name in file_path for file_name in expected_file_names)


@pytest.mark.parametrize(
    "market_role",
    [MarketRole.SYSTEM_OPERATOR, MarketRole.DATAHUB_ADMINISTRATOR],
)
def test_execute_quarterly_time_series__when_system_operator_or_datahub_admin_with_none_energy_supplier_id__scenario__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
    market_role: MarketRole,
):
    # Arrange
    args = copy.deepcopy(standard_wholesale_fixing_scenario_args)
    args.requesting_actor_market_role = market_role
    args.energy_supplier_ids = None
    expected_file_names = [
        "TSSD15_804_02-01-2024_02-01-2024.csv",
        "TSSD15_805_02-01-2024_02-01-2024.csv",
    ]
    expected_columns = [
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.metering_point_id,
        CsvColumnNames.type_of_mp,
        CsvColumnNames.start_date_time,
    ] + [f"ENERGYQUANTITY{i}" for i in range(1, 101)]

    # Act
    execute_quarterly_time_series(spark, dbutils, args)

    # Assert
    actual_files = dbutils.jobs.taskValues.get("quarterly_time_series_files")
    assert len(actual_files) == len(expected_file_names)
    for file_path in actual_files:
        df = spark.read.option("delimiter", ";").csv(file_path, header=True)
        assert df.count() > 0
        assert df.columns == expected_columns
        assert any(file_name in file_path for file_name in expected_file_names)


def test_execute_quarterly_time_series__when_include_basis_data_false__returns_no_file_paths(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    args = copy.deepcopy(standard_wholesale_fixing_scenario_args)
    args.include_basis_data = False

    # Act
    execute_quarterly_time_series(spark, dbutils, args)

    # Assert
    actual_files = dbutils.jobs.taskValues.get("quarterly_time_series_files")
    assert actual_files is None or len(actual_files) == 0


def test_execute_quarterly_time_series__when_energy_supplier_and_balance_fixing__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_balance_fixing_scenario_args: SettlementReportArgs,
    standard_balance_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    args = copy.deepcopy(standard_balance_fixing_scenario_args)
    args.requesting_actor_market_role = MarketRole.ENERGY_SUPPLIER
    energy_supplier_id = (
        standard_balance_fixing_scenario_data_generator.ENERGY_SUPPLIER_IDS[0]
    )
    args.requesting_actor_id = energy_supplier_id
    args.energy_supplier_ids = [energy_supplier_id]
    expected_file_names = [
        f"TSSD15_804_{energy_supplier_id}_DDQ_02-01-2024_02-01-2024.csv",
        f"TSSD15_805_{energy_supplier_id}_DDQ_02-01-2024_02-01-2024.csv",
    ]
    expected_columns = [
        CsvColumnNames.metering_point_id,
        CsvColumnNames.type_of_mp,
        CsvColumnNames.start_date_time,
    ] + [f"ENERGYQUANTITY{i}" for i in range(1, 101)]

    # Act
    execute_quarterly_time_series(spark, dbutils, args)

    # Assert
    actual_files = dbutils.jobs.taskValues.get(key="quarterly_time_series_files")
    assert len(actual_files) == len(expected_file_names)
    for file_path in actual_files:
        df = spark.read.option("delimiter", ";").csv(file_path, header=True)
        assert df.count() > 0
        assert df.columns == expected_columns
        assert any(file_name in file_path for file_name in expected_file_names)

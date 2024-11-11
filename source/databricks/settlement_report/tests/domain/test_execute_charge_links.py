from pyspark.sql import SparkSession
import pytest

from tests.dbutils_fixture import DBUtilsFixture

from tests.data_seeding import standard_wholesale_fixing_scenario_data_generator
from tests.domain.assertion import assert_file_names_and_columns
from settlement_report_job.domain.market_role import MarketRole
import settlement_report_job.domain.report_generator as report_generator
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.domain.csv_column_names import (
    CsvColumnNames,
)
from settlement_report_job.infrastructure.paths import get_report_output_path

def test_execute_charge_links__when_energy_supplier__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_energy_supplier_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    expected_file_names = [
        f"CHARGELINK_804_{standard_wholesale_fixing_scenario_energy_supplier_args.requesting_actor_id}_DDQ_02-01-2024_02-01-2024.csv",
        f"CHARGELINK_805_{standard_wholesale_fixing_scenario_energy_supplier_args.requesting_actor_id}_DDQ_02-01-2024_02-01-2024.csv",
    ]
    expected_columns = [
        CsvColumnNames.metering_point_id,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.charge_type,
        CsvColumnNames.charge_owner_id,
        CsvColumnNames.charge_code,
        CsvColumnNames.charge_quantity,
        CsvColumnNames.charge_link_from_date,
        CsvColumnNames.charge_link_to_date,
    ]
    report_generator_instance = report_generator.ReportGenerator(
        spark, dbutils, standard_wholesale_fixing_scenario_energy_supplier_args
    )

    # Act
    report_generator_instance.execute_charge_links()

    # Assert
    actual_files = dbutils.jobs.taskValues.get(key="charge_links_files")
    assert_file_names_and_columns(
        path=get_report_output_path(
            standard_wholesale_fixing_scenario_energy_supplier_args
        ),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


def test_execute_charge_links__when_grid_access_provider__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_grid_access_provider_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    expected_file_names = [
        f"CHARGELINK_804_{standard_wholesale_fixing_scenario_grid_access_provider_args.requesting_actor_id}_DDM_02-01-2024_02-01-2024.csv",
        f"CHARGELINK_805_{standard_wholesale_fixing_scenario_grid_access_provider_args.requesting_actor_id}_DDM_02-01-2024_02-01-2024.csv",
    ]
    expected_columns = [
        CsvColumnNames.metering_point_id,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.charge_type,
        CsvColumnNames.charge_owner_id,
        CsvColumnNames.charge_code,
        CsvColumnNames.charge_quantity,
        CsvColumnNames.charge_link_from_date,
        CsvColumnNames.charge_link_to_date,
    ]
    report_generator_instance = report_generator.ReportGenerator(
        spark, dbutils, standard_wholesale_fixing_scenario_grid_access_provider_args
    )

    # Act
    report_generator_instance.execute_charge_links()

    # Assert
    actual_files = dbutils.jobs.taskValues.get("charge_links_files")
    assert_file_names_and_columns(
        path=get_report_output_path(
            standard_wholesale_fixing_scenario_grid_access_provider_args
        ),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


@pytest.mark.parametrize(
    "market_role",
    [MarketRole.SYSTEM_OPERATOR, MarketRole.DATAHUB_ADMINISTRATOR],
)
def test_execute_charge_links__when_system_operator_or_datahub_admin_with_one_energy_supplier_id__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
    market_role: MarketRole,
):
    # Arrange
    args = standard_wholesale_fixing_scenario_args
    args.requesting_actor_market_role = market_role
    energy_supplier_id = (
        standard_wholesale_fixing_scenario_data_generator.ENERGY_SUPPLIER_IDS[0]
    )
    args.energy_supplier_ids = [energy_supplier_id]
    expected_file_names = [
        f"CHARGELINK_804_{energy_supplier_id}_02-01-2024_02-01-2024.csv",
        f"CHARGELINK_805_{energy_supplier_id}_02-01-2024_02-01-2024.csv",
    ]
    expected_columns = [
        CsvColumnNames.metering_point_id,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.charge_type,
        CsvColumnNames.charge_owner_id,
        CsvColumnNames.charge_code,
        CsvColumnNames.charge_quantity,
        CsvColumnNames.charge_link_from_date,
        CsvColumnNames.charge_link_to_date,
        CsvColumnNames.energy_supplier_id,
    ]
    report_generator_instance = report_generator.ReportGenerator(spark, dbutils, args)

    # Act
    report_generator_instance.execute_charge_links()

    # Assert
    actual_files = dbutils.jobs.taskValues.get("charge_links_files")
    assert_file_names_and_columns(
        path=get_report_output_path(args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


@pytest.mark.parametrize(
    "market_role",
    [MarketRole.SYSTEM_OPERATOR, MarketRole.DATAHUB_ADMINISTRATOR],
)
def test_execute_charge_links__when_system_operator_or_datahub_admin_with_none_energy_supplier_id__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
    market_role: MarketRole,
):
    # Arrange
    args = standard_wholesale_fixing_scenario_args
    args.requesting_actor_market_role = market_role
    args.energy_supplier_ids = None
    expected_file_names = [
        "CHARGELINK_804_02-01-2024_02-01-2024.csv",
        "CHARGELINK_805_02-01-2024_02-01-2024.csv",
    ]
    expected_columns = [
        CsvColumnNames.metering_point_id,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.charge_type,
        CsvColumnNames.charge_owner_id,
        CsvColumnNames.charge_code,
        CsvColumnNames.charge_quantity,
        CsvColumnNames.charge_link_from_date,
        CsvColumnNames.charge_link_to_date,
        CsvColumnNames.energy_supplier_id,
    ]
    report_generator_instance = report_generator.ReportGenerator(spark, dbutils, args)

    # Act
    report_generator_instance.execute_charge_links()

    # Assert
    actual_files = dbutils.jobs.taskValues.get("charge_links_files")
    assert_file_names_and_columns(
        path=get_report_output_path(args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


def test_execute_charge_links__when_include_basis_data_false__returns_no_file_paths(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    args = standard_wholesale_fixing_scenario_args
    args.include_basis_data = False
    report_generator_instance = report_generator.ReportGenerator(spark, dbutils, args)

    # Act
    report_generator_instance.execute_charge_links()

    # Assert
    actual_files = dbutils.jobs.taskValues.get("charge_links_files")
    assert actual_files is None or len(actual_files) == 0

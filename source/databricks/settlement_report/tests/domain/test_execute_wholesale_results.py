from pyspark.sql import SparkSession
import pytest


from data_seeding import (
    standard_wholesale_fixing_scenario_data_generator,
)
from dbutils_fixture import DBUtilsFixture
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.report_generator import (
    execute_wholesale_results,
)
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.domain.csv_column_names import (
    CsvColumnNames,
)


@pytest.fixture(scope="function", autouse=True)
def reset_task_values(dbutils: DBUtilsFixture):
    yield
    print("Resetting task values")
    dbutils.jobs.taskValues.reset()


def test_execute_wholesale_results__when_energy_supplier_and_split_by_grid_area_is_false__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    args = standard_wholesale_fixing_scenario_args
    args.split_report_by_grid_area = False
    args.requesting_actor_market_role = MarketRole.ENERGY_SUPPLIER
    energy_supplier_id = (
        standard_wholesale_fixing_scenario_data_generator.ENERGY_SUPPLIER_IDS[0]
    )
    args.requesting_actor_id = energy_supplier_id
    args.energy_supplier_ids = [energy_supplier_id]
    expected_file_name = [
        f"RESULTWHOLESALE_flere-net_{energy_supplier_id}_DDQ_02-01-2024_02-01-2024.csv",
    ]
    expected_columns = [
        CsvColumnNames.calculation_type,
        CsvColumnNames.correction_settlement_number,
        CsvColumnNames.grid_area_code,
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.time,
        CsvColumnNames.resolution,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.settlement_method,
        CsvColumnNames.quantity_unit,
        CsvColumnNames.currency,
        CsvColumnNames.quantity,
        CsvColumnNames.price,
        CsvColumnNames.amount,
        CsvColumnNames.charge_type,
        CsvColumnNames.charge_code,
        CsvColumnNames.charge_owner_id,
    ]

    # Act
    execute_wholesale_results(spark, dbutils, args)

    # Assert
    actual_files = dbutils.jobs.taskValues.get(key="wholesale_result_files")
    assert len(actual_files) == len(expected_file_name)
    for file_path in actual_files:
        df = spark.read.csv(file_path, header=True)
        assert df.count() > 0
        assert df.columns == expected_columns
        assert any(file_name in file_path for file_name in expected_file_name)

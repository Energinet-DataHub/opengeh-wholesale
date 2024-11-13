from pyspark.sql import SparkSession
import pytest

from dbutils_fixture import DBUtilsFixture
from assertion import assert_file_names_and_columns

from data_seeding.standard_wholesale_fixing_scenario_data_generator import (
    CHARGE_OWNER_ID_WITHOUT_TAX,
)
from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.domain.utils.report_data_type import ReportDataType
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from settlement_report_job.domain.utils.csv_column_names import (
    CsvColumnNames,
)
from settlement_report_job.entry_points.tasks.wholesale_results_task import (
    WholesaleResultsTask,
)
from settlement_report_job.infrastructure.paths import get_report_output_path
from utils import (
    get_market_role_in_file_name,
    get_start_date,
    get_end_date,
    cleanup_output_path,
    get_actual_files,
)


@pytest.fixture(scope="function", autouse=True)
def reset_task_values(settlement_reports_output_path: str):
    yield
    cleanup_output_path(
        settlement_reports_output_path=settlement_reports_output_path,
    )


def test_execute_wholesale_results__when_energy_supplier_and_split_by_grid_area_is_false__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_energy_supplier_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    args = standard_wholesale_fixing_scenario_energy_supplier_args
    args.split_report_by_grid_area = False
    args.requesting_actor_market_role = MarketRole.ENERGY_SUPPLIER

    market_role_in_file_name = get_market_role_in_file_name(
        args.requesting_actor_market_role
    )

    start_time = get_start_date(args.period_start)
    end_time = get_end_date(args.period_end)

    energy_supplier_id = args.energy_supplier_ids[0]

    expected_file_names = [
        f"RESULTWHOLESALE_flere-net_{energy_supplier_id}_{market_role_in_file_name}_{start_time}_{end_time}.csv",
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
        CsvColumnNames.energy_quantity,
        CsvColumnNames.price,
        CsvColumnNames.amount,
        CsvColumnNames.charge_type,
        CsvColumnNames.charge_code,
        CsvColumnNames.charge_owner_id,
    ]
    task = WholesaleResultsTask(spark, dbutils, args)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.WholesaleResults,
        args=args,
    )
    assert_file_names_and_columns(
        path=get_report_output_path(args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


def test_execute_wholesale_results__when_energy_supplier_and_split_by_grid_area_is_true__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_energy_supplier_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    args = standard_wholesale_fixing_scenario_energy_supplier_args
    args.split_report_by_grid_area = True
    args.requesting_actor_market_role = MarketRole.ENERGY_SUPPLIER

    market_role_in_file_name = get_market_role_in_file_name(
        args.requesting_actor_market_role
    )

    start_time = get_start_date(args.period_start)
    end_time = get_end_date(args.period_end)

    grid_area_codes = list(args.calculation_id_by_grid_area.keys())
    grid_area_code_1 = grid_area_codes[0]
    grid_area_code_2 = grid_area_codes[1]

    energy_supplier_id = args.energy_supplier_ids[0]

    expected_file_names = [
        f"RESULTWHOLESALE_{grid_area_code_1}_{energy_supplier_id}_{market_role_in_file_name}_{start_time}_{end_time}.csv",
        f"RESULTWHOLESALE_{grid_area_code_2}_{energy_supplier_id}_{market_role_in_file_name}_{start_time}_{end_time}.csv",
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
        CsvColumnNames.energy_quantity,
        CsvColumnNames.price,
        CsvColumnNames.amount,
        CsvColumnNames.charge_type,
        CsvColumnNames.charge_code,
        CsvColumnNames.charge_owner_id,
    ]
    task = WholesaleResultsTask(spark, dbutils, args)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.WholesaleResults,
        args=args,
    )
    assert_file_names_and_columns(
        path=get_report_output_path(args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


@pytest.mark.parametrize(
    "market_role",
    [
        pytest.param(
            MarketRole.SYSTEM_OPERATOR, id="system_operator return correct file names"
        ),
        pytest.param(
            MarketRole.DATAHUB_ADMINISTRATOR,
            id="datahub_administrator return correct file names",
        ),
    ],
)
def test_when_market_role_is(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
    market_role: MarketRole,
):
    # Arrange
    args = standard_wholesale_fixing_scenario_args
    args.split_report_by_grid_area = True
    args.requesting_actor_market_role = market_role
    args.energy_supplier_ids = None
    args.requesting_actor_id = CHARGE_OWNER_ID_WITHOUT_TAX

    start_time = get_start_date(args.period_start)
    end_time = get_end_date(args.period_end)

    grid_area_codes = list(args.calculation_id_by_grid_area.keys())
    grid_area_code_1 = grid_area_codes[0]
    grid_area_code_2 = grid_area_codes[1]

    expected_file_names = [
        f"RESULTWHOLESALE_{grid_area_code_1}_{start_time}_{end_time}.csv",
        f"RESULTWHOLESALE_{grid_area_code_2}_{start_time}_{end_time}.csv",
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
        CsvColumnNames.energy_quantity,
        CsvColumnNames.price,
        CsvColumnNames.amount,
        CsvColumnNames.charge_type,
        CsvColumnNames.charge_code,
        CsvColumnNames.charge_owner_id,
    ]
    task = WholesaleResultsTask(spark, dbutils, args)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.WholesaleResults,
        args=args,
    )
    assert_file_names_and_columns(
        path=get_report_output_path(args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


def test_when_market_role_is_grid_access_provider_return_correct_file_name(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    args = standard_wholesale_fixing_scenario_args
    args.split_report_by_grid_area = True
    args.requesting_actor_market_role = MarketRole.GRID_ACCESS_PROVIDER
    args.energy_supplier_ids = None

    market_role_in_file_name = get_market_role_in_file_name(
        args.requesting_actor_market_role
    )

    start_time = get_start_date(args.period_start)
    end_time = get_end_date(args.period_end)

    grid_area_codes = list(args.calculation_id_by_grid_area.keys())
    grid_area_code_1 = grid_area_codes[0]
    grid_area_code_2 = grid_area_codes[1]

    expected_file_names = [
        f"RESULTWHOLESALE_{grid_area_code_1}_{args.requesting_actor_id}_{market_role_in_file_name}_{start_time}_{end_time}.csv",
        f"RESULTWHOLESALE_{grid_area_code_2}_{args.requesting_actor_id}_{market_role_in_file_name}_{start_time}_{end_time}.csv",
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
        CsvColumnNames.energy_quantity,
        CsvColumnNames.price,
        CsvColumnNames.amount,
        CsvColumnNames.charge_type,
        CsvColumnNames.charge_code,
        CsvColumnNames.charge_owner_id,
    ]
    task = WholesaleResultsTask(spark, dbutils, args)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.WholesaleResults,
        args=args,
    )
    assert_file_names_and_columns(
        path=get_report_output_path(args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )

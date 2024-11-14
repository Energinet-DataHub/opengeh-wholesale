from pyspark.sql import SparkSession
import pytest

from assertion import assert_file_names_and_columns
from data_seeding import standard_wholesale_fixing_scenario_data_generator
from dbutils_fixture import DBUtilsFixture

from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.domain.utils.report_data_type import ReportDataType
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from settlement_report_job.domain.utils.csv_column_names import (
    CsvColumnNames,
)
from settlement_report_job.entry_points.tasks.charge_price_points_task import (
    ChargePricePointsTask,
)
from settlement_report_job.infrastructure.paths import get_report_output_path
from utils import get_actual_files

expected_columns = [
    CsvColumnNames.charge_type,
    CsvColumnNames.charge_owner_id,
    CsvColumnNames.charge_code,
    CsvColumnNames.resolution,
    CsvColumnNames.is_tax,
    CsvColumnNames.time,
    f"{CsvColumnNames.energy_price}1",
    f"{CsvColumnNames.energy_price}2",
    f"{CsvColumnNames.energy_price}3",
    f"{CsvColumnNames.energy_price}4",
    f"{CsvColumnNames.energy_price}5",
    f"{CsvColumnNames.energy_price}6",
    f"{CsvColumnNames.energy_price}7",
    f"{CsvColumnNames.energy_price}8",
    f"{CsvColumnNames.energy_price}9",
    f"{CsvColumnNames.energy_price}10",
    f"{CsvColumnNames.energy_price}11",
    f"{CsvColumnNames.energy_price}12",
    f"{CsvColumnNames.energy_price}13",
    f"{CsvColumnNames.energy_price}14",
    f"{CsvColumnNames.energy_price}15",
    f"{CsvColumnNames.energy_price}16",
    f"{CsvColumnNames.energy_price}17",
    f"{CsvColumnNames.energy_price}18",
    f"{CsvColumnNames.energy_price}19",
    f"{CsvColumnNames.energy_price}20",
    f"{CsvColumnNames.energy_price}21",
    f"{CsvColumnNames.energy_price}22",
    f"{CsvColumnNames.energy_price}23",
    f"{CsvColumnNames.energy_price}24",
    f"{CsvColumnNames.energy_price}25",
]


def test_execute_charge_price_points__when_energy_supplier__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_energy_supplier_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    expected_file_names = [
        f"CHARGEPRICE_804_{standard_wholesale_fixing_scenario_energy_supplier_args.requesting_actor_id}_DDQ_02-01-2024_02-01-2024.csv",
        f"CHARGEPRICE_805_{standard_wholesale_fixing_scenario_energy_supplier_args.requesting_actor_id}_DDQ_02-01-2024_02-01-2024.csv",
    ]

    task = ChargePricePointsTask(
        spark, dbutils, standard_wholesale_fixing_scenario_energy_supplier_args
    )

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.ChargePricePoints,
        args=standard_wholesale_fixing_scenario_energy_supplier_args,
    )

    assert_file_names_and_columns(
        path=get_report_output_path(
            standard_wholesale_fixing_scenario_energy_supplier_args
        ),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


def test_execute_charge_price_points__when_grid_access_provider__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_grid_access_provider_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    expected_file_names = [
        f"CHARGEPRICE_804_{standard_wholesale_fixing_scenario_grid_access_provider_args.requesting_actor_id}_DDM_02-01-2024_02-01-2024.csv",
        f"CHARGEPRICE_805_{standard_wholesale_fixing_scenario_grid_access_provider_args.requesting_actor_id}_DDM_02-01-2024_02-01-2024.csv",
    ]

    task = ChargePricePointsTask(
        spark, dbutils, standard_wholesale_fixing_scenario_grid_access_provider_args
    )

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.ChargePricePoints,
        args=standard_wholesale_fixing_scenario_grid_access_provider_args,
    )
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
def test_execute_charge_price_points__when_system_operator_or_datahub_admin_with_one_energy_supplier_id__returns_expected(
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
        f"CHARGEPRICE_804_{energy_supplier_id}_02-01-2024_02-01-2024.csv",
        f"CHARGEPRICE_805_{energy_supplier_id}_02-01-2024_02-01-2024.csv",
    ]

    task = ChargePricePointsTask(
        spark, dbutils, standard_wholesale_fixing_scenario_args
    )

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.ChargePricePoints,
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
    [MarketRole.SYSTEM_OPERATOR, MarketRole.DATAHUB_ADMINISTRATOR],
)
def test_execute_charge_price_points__when_system_operator_or_datahub_admin_with_none_energy_supplier_id__returns_expected(
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
        "CHARGEPRICE_804_02-01-2024_02-01-2024.csv",
        "CHARGEPRICE_805_02-01-2024_02-01-2024.csv",
    ]

    task = ChargePricePointsTask(
        spark, dbutils, standard_wholesale_fixing_scenario_args
    )

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.ChargePricePoints,
        args=args,
    )

    assert_file_names_and_columns(
        path=get_report_output_path(args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


def test_execute_charge_price_points__when_include_basis_data_false__returns_no_file_paths(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    args = standard_wholesale_fixing_scenario_args
    args.include_basis_data = False
    task = ChargePricePointsTask(
        spark, dbutils, standard_wholesale_fixing_scenario_args
    )

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.ChargePricePoints,
        args=args,
    )
    assert actual_files is None or len(actual_files) == 0

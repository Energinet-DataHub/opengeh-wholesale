from unittest.mock import Mock

import pytest
from pyspark.sql import SparkSession, functions as F
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.domain.csv_column_names import (
    CsvColumnNames,
    EphemeralColumns,
)
from settlement_report_job.domain.monthly_amounts.read_and_filter import (
    extend_monthly_amounts_per_charge_columns_for_union,
)
from settlement_report_job.wholesale.data_values.calculation_type import (
    CalculationTypeDataProductValue,
)
from settlement_report_job.wholesale.data_values.charge_type import (
    ChargeTypeDataProductValue,
)
import test_factories.default_test_data_spec as default_data
import test_factories.monthly_amounts_per_charge_factory as monthly_amounts_per_charge_factory
import test_factories.total_monthly_amounts_factory as total_monthly_amounts_factory

from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.monthly_amounts.prepare_for_csv import (
    prepare_for_csv,
)
from settlement_report_job.wholesale.column_names import DataProductColumnNames

DEFAULT_FROM_DATE = default_data.DEFAULT_FROM_DATE
DEFAULT_TO_DATE = default_data.DEFAULT_TO_DATE
DATAHUB_ADMINISTRATOR_ID = "1234567890123"
SYSTEM_OPERATOR_ID = "3333333333333"
NOT_SYSTEM_OPERATOR_ID = "4444444444444"
DEFAULT_TIME_ZONE = "Europe/Copenhagen"
DEFAULT_CALCULATION_ID = "12345678-6f20-40c5-9a95-f419a1245d7e"


@pytest.mark.parametrize("create_ephemeral_grid_loss_column", [True, False])
def test_prepare_for_csv__returns_expected_columns(
    spark: SparkSession,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    create_ephemeral_grid_loss_column: bool,
) -> None:
    # Arrange
    testing_spec = default_data.create_total_monthly_amounts_row()
    monthly_amounts_per_charge = monthly_amounts_per_charge_factory.create(
        spark, testing_spec
    )
    monthly_amounts_per_charge = extend_monthly_amounts_per_charge_columns_for_union(
        monthly_amounts_per_charge
    )

    expected_columns = [
        CsvColumnNames.calculation_type,
        CsvColumnNames.correction_settlement_number,
        CsvColumnNames.grid_area_code,
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.time,
        CsvColumnNames.resolution,
        CsvColumnNames.quantity_unit,
        CsvColumnNames.currency,
        CsvColumnNames.amount,
        CsvColumnNames.charge_type,
        CsvColumnNames.charge_code,
        CsvColumnNames.charge_owner_id,
    ]

    if create_ephemeral_grid_loss_column:
        expected_columns.append(EphemeralColumns.grid_area_code_partitioning)

    # Act
    actual_df = prepare_for_csv(
        monthly_amounts_per_charge, create_ephemeral_grid_loss_column
    )

    # Assert
    assert expected_columns == actual_df.columns


@pytest.mark.parametrize(
    "calculation_type, expected_process_variant",
    [
        pytest.param(
            CalculationTypeDataProductValue.FIRST_CORRECTION_SETTLEMENT,
            "1ST",
            id="when calculation type is first_correction_settlement, then process variant is 1ST",
        ),
        pytest.param(
            CalculationTypeDataProductValue.SECOND_CORRECTION_SETTLEMENT,
            "2ND",
            id="when calculation type is second_correction_settlement, then process variant is 2ND",
        ),
        pytest.param(
            CalculationTypeDataProductValue.THIRD_CORRECTION_SETTLEMENT,
            "3RD",
            id="when calculation type is third_correction_settlement, then process variant is 3RD",
        ),
        pytest.param(
            CalculationTypeDataProductValue.WHOLESALE_FIXING,
            None,
            id="when calculation type is wholesale_fixing, then process variant is None",
        ),
        pytest.param(
            CalculationTypeDataProductValue.BALANCE_FIXING,
            None,
            id="when calculation type is balance_fixing, then process variant is None",
        ),
    ],
)
def test_mapping_of_process_variant(
    spark: SparkSession,
    calculation_type: CalculationTypeDataProductValue,
    expected_process_variant: str,
) -> None:
    # Arrange
    testing_spec = default_data.create_total_monthly_amounts_row(
        calculation_type=calculation_type
    )
    monthly_amounts = monthly_amounts_per_charge_factory.create(spark, testing_spec)
    monthly_amounts = extend_monthly_amounts_per_charge_columns_for_union(
        monthly_amounts
    )

    # Act
    actual = prepare_for_csv(monthly_amounts, create_ephemeral_grid_area_column=False)

    # Assert
    assert actual.collect()[0]["PROCESSVARIANT"] == expected_process_variant


@pytest.mark.parametrize(
    "calculation_type, expected_energy_business_process",
    [
        pytest.param(
            CalculationTypeDataProductValue.BALANCE_FIXING,
            "D04",
            id="when calculation type is balance_fixing, then energy business process is D04",
        ),
        pytest.param(
            CalculationTypeDataProductValue.WHOLESALE_FIXING,
            "D05",
            id="when calculation type is wholesale_fixing, then energy business process is D05",
        ),
        pytest.param(
            CalculationTypeDataProductValue.FIRST_CORRECTION_SETTLEMENT,
            "D32",
            id="when calculation type is first_correction_settlement, then energy business process is D32",
        ),
        pytest.param(
            CalculationTypeDataProductValue.SECOND_CORRECTION_SETTLEMENT,
            "D32",
            id="when calculation type is second_correction_settlement, then energy business process is D32",
        ),
        pytest.param(
            CalculationTypeDataProductValue.THIRD_CORRECTION_SETTLEMENT,
            "D32",
            id="when calculation type is third_correction_settlement, then energy business process is D32",
        ),
    ],
)
def test_mapping_of_energy_business_process(
    spark: SparkSession,
    calculation_type: CalculationTypeDataProductValue,
    expected_energy_business_process: str,
) -> None:
    # Arrange
    testing_spec = default_data.create_total_monthly_amounts_row(
        calculation_type=calculation_type
    )
    monthly_amounts = monthly_amounts_per_charge_factory.create(spark, testing_spec)
    monthly_amounts = extend_monthly_amounts_per_charge_columns_for_union(
        monthly_amounts
    )

    # Act
    actual = prepare_for_csv(monthly_amounts, create_ephemeral_grid_area_column=False)

    # Assert
    assert (
        actual.collect()[0][CsvColumnNames.calculation_type]
        == expected_energy_business_process
    )


@pytest.mark.parametrize(
    "charge_type, expected_charge_type",
    [
        pytest.param(
            ChargeTypeDataProductValue.SUBSCRIPTION,
            "D01",
            id="when charge type is subscription, then charge type is D01",
        ),
        pytest.param(
            ChargeTypeDataProductValue.FEE,
            "D02",
            id="when charge type is fee, then charge type is D02",
        ),
        pytest.param(
            ChargeTypeDataProductValue.TARIFF,
            "D03",
            id="when charge type is tariff, then charge type is D03",
        ),
    ],
)
def test_mapping_of_charge_type(
    spark: SparkSession,
    charge_type: ChargeTypeDataProductValue,
    expected_charge_type: str,
) -> None:
    # Arrange
    testing_spec = default_data.create_total_monthly_amounts_row(
        charge_type=charge_type
    )
    monthly_amounts = monthly_amounts_per_charge_factory.create(spark, testing_spec)
    monthly_amounts = extend_monthly_amounts_per_charge_columns_for_union(
        monthly_amounts
    )

    # Act
    actual = prepare_for_csv(monthly_amounts, create_ephemeral_grid_area_column=False)

    # Assert
    assert actual.collect()[0][CsvColumnNames.charge_type] == expected_charge_type

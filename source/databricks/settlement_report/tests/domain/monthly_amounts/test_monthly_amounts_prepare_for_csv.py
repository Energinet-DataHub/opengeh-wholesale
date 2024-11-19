import pytest
from pyspark.sql import SparkSession
from pyspark.sql.functions import lit

from settlement_report_job.domain.utils.csv_column_names import (
    CsvColumnNames,
    EphemeralColumns,
)
import test_factories.default_test_data_spec as default_data
import test_factories.monthly_amounts_per_charge_factory as monthly_amounts_per_charge_factory

from settlement_report_job.domain.monthly_amounts.prepare_for_csv import (
    prepare_for_csv,
)
from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)

DEFAULT_FROM_DATE = default_data.DEFAULT_FROM_DATE
DEFAULT_TO_DATE = default_data.DEFAULT_TO_DATE
DATAHUB_ADMINISTRATOR_ID = "1234567890123"
SYSTEM_OPERATOR_ID = "3333333333333"
NOT_SYSTEM_OPERATOR_ID = "4444444444444"
DEFAULT_TIME_ZONE = "Europe/Copenhagen"
DEFAULT_CALCULATION_ID = "12345678-6f20-40c5-9a95-f419a1245d7e"


@pytest.mark.parametrize("should_have_one_file_per_grid_area", [True, False])
def test_prepare_for_csv__returns_expected_columns(
    spark: SparkSession,
    should_have_one_file_per_grid_area: bool,
) -> None:
    # Arrange
    monthly_amounts = monthly_amounts_per_charge_factory.create(
        spark, default_data.create_monthly_amounts_per_charge_row()
    )
    monthly_amounts = monthly_amounts.withColumn(
        DataProductColumnNames.resolution, lit("P1M")
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

    if should_have_one_file_per_grid_area:
        expected_columns.append(EphemeralColumns.grid_area_code_partitioning)

    # Act
    actual_df = prepare_for_csv(monthly_amounts, should_have_one_file_per_grid_area)

    # Assert
    assert expected_columns == actual_df.columns

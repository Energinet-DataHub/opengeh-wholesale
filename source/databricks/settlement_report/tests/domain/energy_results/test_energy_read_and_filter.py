import uuid
from datetime import datetime, timedelta
from functools import reduce
from unittest.mock import Mock

import pytest
from pyspark.sql import SparkSession, functions as F
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
import test_factories.default_test_data_spec as default_data
import test_factories.energy_factory as energy_factory
import test_factories.charge_link_periods_factory as charge_links_factory
import test_factories.charge_price_information_periods_factory as charge_price_information_periods
from settlement_report_job.wholesale.data_values import (
    CalculationTypeDataProductValue,
)

from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.energy_results.read_and_filter import (
    read_and_filter_from_view,
)
from settlement_report_job.wholesale.column_names import DataProductColumnNames
from test_factories import latest_calculations_factory
from settlement_report_job.wholesale.data_values import (
    MeteringPointResolutionDataProductValue,
)
from settlement_report_job.domain.csv_column_names import CsvColumnNames

DEFAULT_FROM_DATE = default_data.DEFAULT_FROM_DATE
DEFAULT_TO_DATE = default_data.DEFAULT_TO_DATE
DATAHUB_ADMINISTRATOR_ID = "1234567890123"
SYSTEM_OPERATOR_ID = "3333333333333"
NOT_SYSTEM_OPERATOR_ID = "4444444444444"
DEFAULT_TIME_ZONE = "Europe/Copenhagen"


@pytest.mark.parametrize(
    "request_energy_per_es",
    [
        True,
        False,
    ],
)
def test_energy_read_and_filter_for_energy_results__when_requesting_energy_per_es__returns_correct_columns(
    spark: SparkSession,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    request_energy_per_es: bool,
) -> None:
    # Arrange
    testing_spec = default_data.create_energy_results_data_spec()

    df_energy_per_es_v1 = energy_factory.create_energy_per_es_v1(spark, testing_spec)
    df_energy_v1 = energy_factory.create_energy_v1(spark, testing_spec)

    mock_repository = Mock()
    mock_repository.read_energy.return_value = df_energy_v1
    mock_repository.read_energy_per_es.return_value = df_energy_per_es_v1

    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = (
        MarketRole.DATAHUB_ADMINISTRATOR
        if request_energy_per_es
        else MarketRole.GRID_ACCESS_PROVIDER
    )

    expected_columns = (
        df_energy_per_es_v1.columns if request_energy_per_es else df_energy_v1.columns
    )

    # Act
    actual_df = read_and_filter_from_view(
        args=standard_wholesale_fixing_scenario_args,
        repository=mock_repository,
    )

    # Assert
    for i, column in enumerate(actual_df.columns):
        assert expected_columns[i] == column

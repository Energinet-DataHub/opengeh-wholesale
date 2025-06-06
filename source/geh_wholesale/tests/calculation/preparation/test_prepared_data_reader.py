from unittest import mock
from unittest.mock import patch

from pyspark.sql import SparkSession

import tests.databases.wholesale_results_internal.calculations_storage_model_test_factory as factory
from geh_wholesale.calculation import PreparedDataReader
from geh_wholesale.codelists import CalculationType
from geh_wholesale.databases import wholesale_internal


class TestGetLatestCalculationVersion:
    def test__when_no_calculation_exists__returns_none(self, spark: SparkSession) -> None:
        # Arrange
        repository = wholesale_internal.WholesaleInternalRepository(mock.Mock(), mock.Mock())
        prepared_data_reader = PreparedDataReader(mock.Mock(), repository)
        with patch.object(
            repository,
            repository.read_calculations.__name__,
            return_value=factory.create_empty_calculations(spark),
        ):
            # Act
            actual = prepared_data_reader.get_latest_calculation_version(CalculationType.WHOLESALE_FIXING)

            # Assert
            assert actual is None

    def test__when_calculation_exists__returns_latest_version(self, spark: SparkSession) -> None:
        # Arrange
        repository = wholesale_internal.WholesaleInternalRepository(mock.Mock(), mock.Mock())
        prepared_data_reader = PreparedDataReader(mock.Mock(), repository)

        calculation_type = CalculationType.BALANCE_FIXING
        calculation = factory.create_calculation_row(version=7, calculation_type=calculation_type)
        with patch.object(
            repository,
            repository.read_calculations.__name__,
            return_value=factory.create_calculations(spark, data=[calculation]),
        ):
            # Act
            actual = prepared_data_reader.get_latest_calculation_version(calculation_type)

            # Assert
            assert actual == 7

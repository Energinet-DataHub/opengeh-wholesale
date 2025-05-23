# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
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

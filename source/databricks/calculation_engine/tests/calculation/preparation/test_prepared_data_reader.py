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

from package.calculation import TableReader, PreparedDataReader
import tests.calculation.calculations_factory as factory


class TestGetLatestCalculationVersion:
    def test__when_no_calculation_exists__returns_none(
        self, spark: SparkSession
    ) -> None:
        # Arrange
        table_reader: TableReader = mock.Mock()
        prepared_data_reader = PreparedDataReader(table_reader)
        calculation = factory.create_calculation()
        with patch.object(
            table_reader,
            # table_reader.read_calculations.__name__,
            "read_calculations",
            return_value=factory.create_empty_calculations(spark),
        ):
            # Act
            actual = prepared_data_reader.get_latest_calculation_version(
                calculation.calculation_type
            )

            # Assert
            assert actual is None

    def test__when_calculation_exists__returns_latest_version(
        self, spark: SparkSession
    ) -> None:
        # Arrange
        table_reader: TableReader = mock.Mock()
        prepared_data_reader = PreparedDataReader(table_reader)
        calculation = factory.create_calculation(version=7)
        with patch.object(
            table_reader,
            "read_calculations",
            return_value=factory.create_calculations(spark),
        ):
            # Act
            actual = prepared_data_reader.get_latest_calculation_version(
                calculation.calculation_type
            )

            # Assert
            assert actual == 7

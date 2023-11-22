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

from pyspark.sql import SparkSession

from package.calculation import PreparedDataReader
from package.calculation_input import TableReader
from package.container import Container


def test_container():
    # Arrange
    sut = Container()
    expected_calculation_input_path = "foo"

    sut.calculation_input_path.from_value(expected_calculation_input_path)

    # Act and asserts
    assert sut.calculation_input_path() == expected_calculation_input_path
    assert isinstance(sut.spark(), SparkSession)
    assert isinstance(sut.table_reader(), TableReader)
    assert isinstance(sut.prepared_data_reader(), PreparedDataReader)

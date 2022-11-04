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

import pytest
import subprocess
import unittest
from unittest.mock import patch, Mock

from package.datamigration.data_lake_file_manager import (
    _get_file_system_client,
    download_file,
    download_csv,
)


@patch("package.datamigration.data_lake_file_manager.DataLakeServiceClient")
def test__get_file_system_client__calls_service_client_with_container_name(
    mock_data_lake_service_client,
):

    # Arrange
    dummy_storage_account_name = "my_storage"
    dummy_storage_key = "my_storage"
    dummy_container_name = "my_container"

    # Act
    _get_file_system_client(
        dummy_storage_account_name, dummy_storage_key, dummy_container_name
    )

    # Assert
    mock_data_lake_service_client.return_value.get_file_system_client.assert_called_once_with(
        dummy_container_name
    )


@patch("package.datamigration.data_lake_file_manager.download_file")
def test__download_csv__returned_reader_has_all_items(mock_download_file):

    # Arrange
    row0 = ["c_00", "c_01", "c_02"]
    row1 = ["c_10", "c_11", "c_12"]
    csv_string = f"{row0[0]},{row0[1]},{row0[2]}\r\n{row1[0]},{row1[1]},{row1[2]}\r\n"
    mock_download_file.return_value = str.encode(csv_string)

    # Act
    csv_reader = download_csv("", "", "", "")

    # Assert
    assert row0 == next(csv_reader)
    assert row1 == next(csv_reader)


@patch("package.datamigration.data_lake_file_manager.download_file")
def test__download_csv__when_empty_file__return_empty_content_in_reader(
    mock_download_file,
):

    # Arrange
    mock_download_file.return_value = b""

    # Act
    csv_reader = download_csv("", "", "", "")

    # Assert
    assert csv_reader.line_num == 0


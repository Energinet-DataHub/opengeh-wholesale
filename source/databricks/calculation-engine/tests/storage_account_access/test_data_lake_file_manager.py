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

from unittest.mock import patch

from package.storage_account_access.data_lake_file_manager import DataLakeFileManager

DUMMY_STORAGE_ACCOUNT_NAME = "my_storage"
DUMMY_STORAGE_KEY = "my_storage"
DUMMY_CONTAINER_NAME = "my_container"


@patch("package.storage_account_access.data_lake_file_manager.DataLakeServiceClient")
def test__get_file_system_client__calls_service_client_with_container_name(
    mock_data_lake_service_client,
):
    # Act
    DataLakeFileManager(
        DUMMY_STORAGE_ACCOUNT_NAME, DUMMY_STORAGE_KEY, DUMMY_CONTAINER_NAME
    )

    # Assert
    mock_data_lake_service_client.return_value.get_file_system_client.assert_called_once_with(
        DUMMY_CONTAINER_NAME
    )


@patch(
    "package.storage_account_access.data_lake_file_manager.DataLakeFileManager.download_file"
)
@patch("package.storage_account_access.data_lake_file_manager.DataLakeServiceClient")
def test__download_csv__returned_reader_has_all_items(
    mock_data_lake_service_client, mock_download_file
):
    # Arrange
    row0 = ["c_00", "c_01", "c_02"]
    row1 = ["c_10", "c_11", "c_12"]
    csv_string = f"{row0[0]},{row0[1]},{row0[2]}\r\n{row1[0]},{row1[1]},{row1[2]}\r\n"
    mock_download_file.return_value = str.encode(csv_string)

    file_manager = DataLakeFileManager(
        DUMMY_STORAGE_ACCOUNT_NAME, DUMMY_STORAGE_KEY, DUMMY_CONTAINER_NAME
    )

    # Act
    csv_reader = file_manager.download_csv("filename")

    # Assert
    assert row0 == next(csv_reader)
    assert row1 == next(csv_reader)


@patch(
    "package.storage_account_access.data_lake_file_manager.DataLakeFileManager.download_file"
)
@patch("package.storage_account_access.data_lake_file_manager.DataLakeServiceClient")
def test__download_csv__when_empty_file__return_empty_content_in_reader(
    mock_data_lake_service_client, mock_download_file
):
    # Arrange
    mock_download_file.return_value = b""
    file_manager = DataLakeFileManager(
        DUMMY_STORAGE_ACCOUNT_NAME, DUMMY_STORAGE_KEY, DUMMY_CONTAINER_NAME
    )

    # Act
    csv_reader = file_manager.download_csv("")

    # Assert
    assert csv_reader.line_num == 0

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

from azure.identity import DefaultAzureCredential
from azure.storage.filedatalake import DataLakeServiceClient, FileSystemClient
from io import StringIO
import csv


def _get_file_system_client(
    storage_account_name: str, storage_account_key: str, container_name: str
) -> FileSystemClient:
    datalake_service_client = DataLakeServiceClient(
        storage_account_name, storage_account_key
    )
    return datalake_service_client.get_file_system_client(container_name)


def download_file(
    data_storage_account_name: str,
    data_storage_account_key: str,
    container_name: str,
    file_name: str,
) -> bytes:
    file_system_client = _get_file_system_client(
        data_storage_account_name,
        data_storage_account_key,
        container_name,
    )

    file_client = file_system_client.get_file_client(file_name)
    download = file_client.download_file()
    downloaded_bytes = download.readall()
    return downloaded_bytes


def download_csv(
    data_storage_account_name: str,
    data_storage_account_key: str,
    container_name: str,
    file_name: str,
):
    downloaded_bytes = download_file(
        data_storage_account_name,
        data_storage_account_key,
        container_name,
        file_name,
    )

    string_data = StringIO(downloaded_bytes.decode())
    csv.reader(string_data, dialect="excel")

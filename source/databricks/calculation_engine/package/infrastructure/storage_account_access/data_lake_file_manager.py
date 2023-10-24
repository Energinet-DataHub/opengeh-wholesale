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

import csv
from io import StringIO

from azure.storage.filedatalake import DataLakeServiceClient
from azure.identity import ClientSecretCredential
from package.infrastructure import paths
from typing import Any


class DataLakeFileManager:
    def __init__(
        self,
        data_storage_account_name: str,
        credential: ClientSecretCredential,
        container_name: str,
    ):
        data_storage_account_url = paths.get_storage_account_url(
            data_storage_account_name
        )
        self.file_system_client = DataLakeServiceClient(
            data_storage_account_url, credential=credential
        ).get_file_system_client(container_name)

    def download_file(self, file_name: str) -> bytes:
        file_client = self.file_system_client.get_file_client(file_name)
        download = file_client.download_file()
        downloaded_bytes = download.readall()
        return downloaded_bytes

    def append_data(self, file_name: str, data: str) -> None:
        file_client = self.file_system_client.get_file_client(file_name)
        file_client.get_file_properties().size
        filesize_previous = file_client.get_file_properties().size
        file_client.append_data(data, offset=filesize_previous, length=len(data))
        file_client.flush_data(filesize_previous + len(data))

    def get_file_size(self, file_name: str) -> int:
        file_client = self.file_system_client.get_file_client(file_name)
        return file_client.get_file_properties().size

    def download_csv(self, file_name: str) -> Any:
        downloaded_bytes = self.download_file(file_name)
        string_data = StringIO(downloaded_bytes.decode())
        return csv.reader(string_data, dialect="excel")

    def exists_file(self, file_name: str) -> bool:
        file_client = self.file_system_client.get_file_client(file_name)
        return file_client.exists()

    def create_file(self, file_name: str) -> None:
        file_client = self.file_system_client.get_file_client(file_name)
        file_client.create_file()

    def delete_file(self, file_name: str) -> None:
        file_client = self.file_system_client.get_file_client(file_name)
        file_client.delete_file()

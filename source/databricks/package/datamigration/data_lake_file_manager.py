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


class Data_lake_file_manager:
    def __init__(self, data_storage_account_name, data_storage_account_key, container_name,):
        self.file_system_client = DataLakeServiceClient(
            data_storage_account_name, data_storage_account_key
        ).get_file_system_client(container_name)

    def download_file(self, file_name: str) -> bytes:
        file_client = self.file_system_client.get_file_client(file_name)
        download = file_client.download_file()
        downloaded_bytes = download.readall()
        return downloaded_bytes

    def download_csv(self, file_name: str):
        downloaded_bytes = self.download_file(file_name)
        string_data = StringIO(downloaded_bytes.decode())
        return csv.reader(string_data, dialect="excel")

    def create_file(self, file_name: str):
        file_client = self.file_system_client.get_file_client(file_name)
        file_client.create_file()

    def delete_file(self, file_name: str):
        file_client = self.file_system_client.get_file_client(file_name)
        file_client.delete_file()

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

from azure.identity import ClientSecretCredential
from pyspark.sql import SparkSession


class MigrationScriptArgs:
    """Input arguments for the data lake migration scripts.

    This class is shared across all scripts. The signature
    of existing scripts doesn't need to be updated when new
    members are added to this class.
    """

    def __init__(
        self,
        data_storage_account_url: str,
        data_storage_account_name: str,
        data_storage_container_name: str,
        data_storage_credential: ClientSecretCredential,
        spark: SparkSession,
        calculation_input_folder: str,
    ) -> None:
        self.storage_account_url = data_storage_account_url
        self.storage_account_name = data_storage_account_name
        self.storage_container_path = f"abfss://{data_storage_container_name}@{self.storage_account_name}.dfs.core.windows.net"
        self.storage_credential = data_storage_credential
        self.spark = spark
        self.calculation_input_folder = calculation_input_folder

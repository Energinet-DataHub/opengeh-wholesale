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

import dbutils
from azure.identity import ClientSecretCredential


def get_client_secret_credential() -> ClientSecretCredential:
    tenant_id = dbutils.secrets.get(scope="tenant-id-scope", key="tenant_id")
    client_id = dbutils.secrets.get(scope="wholesale-spn-id-scope", key="spn_app_id")
    client_secret = dbutils.secrets.get(
        scope="wholesale-spn-secret-scope", key="spn_app_secret"
    )
    return ClientSecretCredential(tenant_id, client_id, client_secret)

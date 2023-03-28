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
from package.environment_variables import (
    get_env_variables_or_throw,
    EnvironmentVariable,
)


def get_service_principal_credential() -> ClientSecretCredential:
    required_env_variables = [
        EnvironmentVariable.TENANT_ID,
        EnvironmentVariable.SPN_APP_ID,
        EnvironmentVariable.SPN_APP_SECRET,
    ]
    env_vars = get_env_variables_or_throw(required_env_variables)

    return ClientSecretCredential(
        env_vars[EnvironmentVariable.TENANT_ID],
        env_vars[EnvironmentVariable.SPN_APP_ID],
        env_vars[EnvironmentVariable.SPN_APP_SECRET],
    )

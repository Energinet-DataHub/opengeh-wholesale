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


# This class contains all relevant feature flag management logic for the wholesale calculation.
# The bussiness uses the term release toggle to describe feature flags.
# The actual feature flags can be found in Azure -> App Configuration -> Operations -> Feature manager.


from azure.appconfiguration.provider import WatchKey, load
from azure.identity import ClientSecretCredential
from featuremanagement import FeatureManager


class FeatureManagerFactory:
    def __init__(self, client_secret_credential: ClientSecretCredential, feature_manager_endpoint: str) -> None:
        self._client_secret_credential = client_secret_credential
        self._feature_manager_endpoint = feature_manager_endpoint

    def build(self) -> FeatureManager:
        """Build feature manager."""
        # feature_flag_enabled makes it so that the provider will load feature flags from Azure App Configuration
        # feature_flag_refresh_enabled makes it so that the provider will refresh feature flags from Azure App Configuration, when the refresh operation is triggered

        config = load(
            endpoint=self._feature_manager_endpoint,
            credential=self._client_secret_credential,
            refresh_on=[WatchKey(FeatureFlags.measuredata_measurements)],
            refresh_interval=30,  # The default is 30 seconds.
            feature_flag_enabled=True,
            feature_flag_refresh_enabled=True,
        )

        return FeatureManager(config)


class FeatureFlags:
    measuredata_measurements = "MEASUREDATA-MEASUREMENTS"

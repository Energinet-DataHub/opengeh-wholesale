from azure.appconfiguration.provider import load
from azure.identity import ClientSecretCredential
from featuremanagement import FeatureManager


# This class contains all relevant feature flag management logic for the wholesale calculation.
# The actual feature flags can be found in Azure -> App Configuration -> Operations -> Feature manager.
class FeatureManagerFactory:
    def __init__(self, client_secret_credential: ClientSecretCredential, feature_manager_endpoint: str) -> None:
        self._client_secret_credential = client_secret_credential
        self._feature_manager_endpoint = feature_manager_endpoint

    def build(self) -> FeatureManager:
        config = load(
            #
            # The URL of your Azure App Configuration instance. This tells the SDK where to connect to fetch configuration and feature flags.
            endpoint=self._feature_manager_endpoint,
            #
            # The credential used to authenticate with Azure App Configuration.
            credential=self._client_secret_credential,
            #
            # Enables loading of feature flags from Azure App Configuration, not just regular key-values.
            feature_flag_enabled=True,
            #
            # Enables automatic refreshing of feature flags when a refresh is triggered (e.g., by a change in a watched key).
            # However, we don't want these values to change during the job execution there it is False.
            feature_flag_refresh_enabled=False,
        )

        return FeatureManager(config)


class FeatureFlags:
    measuredata_measurements = "MEASUREDATA-MEASUREMENTS"

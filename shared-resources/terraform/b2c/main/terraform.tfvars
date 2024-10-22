# This file contains values that are the same across all environments
# For environment-specific values, refer to /env/<env_name>/environment.auto.tfvars

# NOTE: Changing OpenIdConfigurationUrl is not supported through the API.
# In case of change, increment the identifier version, e.g. MitIDv4 > MitIDv5 in the AddMitIdProvider.ps1 file in the scripts folder
mitid_config_url="https://pp.netseidbroker.dk/op/.well-known/openid-configuration"
mitid_client_id="4d7431b6-8187-4b34-a5fd-9cb9339cd6c2"

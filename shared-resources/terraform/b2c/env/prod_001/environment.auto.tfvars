# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/terraform.tfvars
b2c_tenant_id = "4b8c3f88-6cca-480c-af02-b2d2f220913f"
b2c_client_id = "d275dbf4-3575-43de-913a-7309428dc5be"
b2c_tenant_name = "b2cshresprodwe001"

# NOTE: Changing OpenIdConfigurationUrl is not supported through the API.
# In case of change, increment the identifier version, e.g. MitIDv4 > MitIDv5 in the AddMitIdProvider.ps1 file in the scripts folder
mitid_config_url="https://netseidbroker.dk/op/.well-known/openid-configuration"
mitid_client_id="5df1e4c2-757f-4cee-a91a-a249b580bf05"

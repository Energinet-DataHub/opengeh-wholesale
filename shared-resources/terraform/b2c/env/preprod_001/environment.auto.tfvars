# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/terraform.tfvars
b2c_tenant_id = "20e7a6b4-86e0-4e7a-a34d-6dc5a75d1982"
b2c_client_id = "4107ba3a-0eb2-4a06-9251-2a5f7782b565"
b2c_tenant_name = "b2cshrespreprodwe001"

# NOTE: Changing OpenIdConfigurationUrl is not supported through the API.
# In case of change, increment the identifier version, e.g. MitIDv4 > MitIDv5 in the AddMitIdProvider.ps1 file in the scripts folder
mitid_config_url="https://netseidbroker.dk/op/.well-known/openid-configuration"
mitid_client_id="47fd67f3-46f5-4d38-a116-0600e03ad214"

# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/terraform.tfvars
apim_b2c_tenant_id                          = "09f452f8-5372-478b-8661-0616584b199e"
virtual_network_resource_group_name         = "rg-network-online-dev-we-002"
virtual_network_name                        = "vnet-datahub-online-dev-we-002"
apim_address_space                          = "10.143.7.128/28"
private_endpoint_address_space              = "10.143.6.0/25"
vnet_integration_address_space              = "10.143.6.128/25"
biztalk_hybrid_connection_hostname          = "datahub.preproduction.biztalk.energinet.local:443"
apim_url                                    = "https://dev002.b2b.datahub3.dk"
developer_security_group_contributor_access = true
developer_security_group_reader_access      = true
databricks_readers_group = {
  id   = "729028915538231"
  name = "SEC-G-Datahub-DevelopersAzure"
}
databricks_contributor_dataplane_group = {
  id   = "729028915538231"
  name = "SEC-G-Datahub-DevelopersAzure"
}

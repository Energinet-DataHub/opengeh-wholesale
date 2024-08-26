# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/terraform.tfvars
apim_b2c_tenant_id                         = "e4dadc22-a954-4dda-bf4b-54eb804233a9"
virtual_network_resource_group_name        = "rg-network-online-test-we-002"
virtual_network_name                       = "vnet-datahub-online-test-we-002"
apim_address_space                         = "10.143.5.128/28"
private_endpoint_address_space             = "10.143.4.0/25"
vnet_integration_address_space             = "10.143.4.128/25"
biztalk_hybrid_connection_hostname         = "datahub.preproduction.biztalk.energinet.local:443"
apim_url                                   = "https://test002.b2b.datahub3.dk"
platform_security_group_contributor_access = true
platform_security_group_reader_access      = true
databricks_readers_group = {
  id   = "729028915538231"
  name = "SEC-G-Datahub-DevelopersAzure"
}
databricks_contributor_dataplane_group = {
  id   = "729028915538231"
  name = "SEC-G-Datahub-DevelopersAzure"
}

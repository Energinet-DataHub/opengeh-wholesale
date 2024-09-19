# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/terraform.tfvars
apim_b2c_tenant_id                       = "284d6f0a-04e7-4260-a1d9-c95661e741ee"
virtual_network_resource_group_name      = "rg-network-online-dev-we-003"
virtual_network_name                     = "vnet-datahub-online-dev-we-003"
apim_address_space                       = "10.143.81.128/28"
private_endpoint_address_space           = "10.143.80.0/25"
vnet_integration_address_space           = "10.143.80.128/25"
biztalk_hybrid_connection_hostname       = "datahub.preproduction.biztalk.energinet.local:443"
apim_url                                 = "https://dev003.b2b.datahub3.dk"
enable_audit_logs                        = false
alert_email_address                      = "d01b5c85.energinet.onmicrosoft.com@emea.teams.ms"
pim_contributor_data_plane_group_name    = "SEC-A-Datahub-Dev-003-Contributor-Dataplane"
pim_contributor_control_plane_group_name = "SEC-A-Datahub-Dev-003-Contributor-Controlplane"
pim_reader_group_name                    = "SEC-A-Datahub-Dev-003-Reader"
databricks_readers_group = {
  id   = "629664400987703"
  name = "SEC-A-Datahub-Dev-003-Reader"
}
databricks_contributor_dataplane_group = {
  id   = "1105450622987696"
  name = "SEC-A-Datahub-Dev-003-Contributor-Dataplane"
}

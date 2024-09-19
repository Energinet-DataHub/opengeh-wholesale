# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/terraform.tfvars
apim_b2c_tenant_id                       = "4b8c3f88-6cca-480c-af02-b2d2f220913f"
virtual_network_resource_group_name      = "rg-network-online-prod-we-001"
virtual_network_name                     = "vnet-datahub-online-prod-we-001"
apim_address_space                       = "10.141.1.128/28"
private_endpoint_address_space           = "10.141.0.0/25"
vnet_integration_address_space           = "10.141.0.128/25"
biztalk_hybrid_connection_hostname       = "datahub.biztalk.energinet.local:443"
apim_url                                 = "https://b2b.datahub3.dk"
enable_audit_logs                        = true
pim_contributor_data_plane_group_name    = "SEC-A-Datahub-Prod-001-Contributor-Dataplane"
pim_contributor_control_plane_group_name = "SEC-A-Datahub-Prod-001-Contributor-Controlplane"
pim_reader_group_name                    = "SEC-A-Datahub-Prod-001-Reader"
alert_email_address                      = "8d8e42fa.energinet.onmicrosoft.com@emea.teams.ms"
azure_maintenance_alerts_email_address   = "ebf4f917.energinet.onmicrosoft.com@emea.teams.ms"
databricks_readers_group = {
  id   = "726131153567802"
  name = "SEC-A-Datahub-Prod-001-Reader"
}
databricks_contributor_dataplane_group = {
  id   = "504707241967571"
  name = "SEC-A-Datahub-Prod-001-Contributor-Dataplane"
}

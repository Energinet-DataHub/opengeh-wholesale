# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/terraform.tfvars
apim_b2c_tenant_id                       = "f0d2a1a1-2b29-4a31-9aa4-bc23f93e8057"
virtual_network_resource_group_name      = "rg-network-online-test-we-001"
virtual_network_name                     = "vnet-datahub-online-test-we-001"
apim_address_space                       = "10.143.1.128/28"
private_endpoint_address_space           = "10.143.0.0/25"
vnet_integration_address_space           = "10.143.0.128/25"
biztalk_hybrid_connection_hostname       = "datahub.preproduction.biztalk.energinet.local:443"
apim_url                                 = "https://test.b2b.datahub3.dk"
enable_audit_logs                        = false
pim_contributor_data_plane_group_name    = "SEC-A-Datahub-Test-001-Contributor-Dataplane"
pim_contributor_control_plane_group_name = "SEC-A-Datahub-Test-001-Contributor-Controlplane"
alert_email_address                      = "d01b5c85.energinet.onmicrosoft.com@emea.teams.ms"
developer_security_group_reader_access   = true
budget_alert_amount                      = 33000 # See issue #2359
databricks_readers_group = {
  id   = "729028915538231"
  name = "SEC-G-Datahub-DevelopersAzure"
}
databricks_contributor_dataplane_group = {
  id   = "318888432738283"
  name = "SEC-A-Datahub-Test-001-Contributor-Dataplane"
}

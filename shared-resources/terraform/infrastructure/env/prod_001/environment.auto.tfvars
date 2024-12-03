# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/terraform.tfvars
apim_b2c_tenant_id                       = "4b8c3f88-6cca-480c-af02-b2d2f220913f"
virtual_network_resource_group_name      = "rg-network-online-prod-we-001"
virtual_network_name                     = "vnet-datahub-online-prod-we-001"
biztalk_hybrid_connection_hostname       = "datahub.biztalk.energinet.local:443"
apim_url                                 = "https://b2b.datahub3.dk"
enable_audit_logs                        = true
pim_contributor_data_plane_group_name    = "SEC-A-Datahub-Prod-001-Contributor-Dataplane"
pim_contributor_control_plane_group_name = "SEC-A-Datahub-Prod-001-Contributor-Controlplane"
pim_reader_group_name                    = "SEC-A-Datahub-Prod-001-Reader"
alert_email_address                      = "8d8e42fa.energinet.onmicrosoft.com@emea.teams.ms"
azure_maintenance_alerts_email_address   = "ebf4f917.energinet.onmicrosoft.com@emea.teams.ms"
budget_alert_amount                      = 420000 # See issue #2359
databricks_readers_group = {
  id   = "726131153567802"
  name = "SEC-A-Datahub-Prod-001-Reader"
}
databricks_contributor_dataplane_group = {
  id   = "504707241967571"
  name = "SEC-A-Datahub-Prod-001-Contributor-Dataplane"
}
log_retention_in_days             = 90
apim_address_prefixes             = ["10.141.94.0/28"]
privateendpoints_address_prefixes = ["10.141.88.0/22"]
vnetintegrations_address_prefixes = ["10.141.92.0/23"]
apim_next_hop_ip_address          = "10.140.96.196"
udr_firewall_next_hop_ip_address  = "10.140.65.196"
enable_autoscale_apim             = true

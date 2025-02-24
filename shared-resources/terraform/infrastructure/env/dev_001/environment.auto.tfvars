# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/terraform.tfvars
apim_b2c_tenant_id                       = "e9aa9b15-7200-441e-b255-927506b3494c"
virtual_network_resource_group_name      = "rg-network-online-dev-we-001"
virtual_network_name                     = "vnet-datahub-online-dev-we-001"
biztalk_hybrid_connection_hostname       = "datahub.preproduction.biztalk.energinet.local:443"
apim_url                                 = "https://dev.b2b.datahub3.dk"
pim_contributor_data_plane_group_name    = "SEC-A-Datahub-Dev-001-Contributor-Dataplane"
pim_contributor_control_plane_group_name = "SEC-A-Datahub-Dev-001-Contributor-Controlplane"
alert_email_address                      = "d01b5c85.energinet.onmicrosoft.com@emea.teams.ms"
enable_audit_logs                        = false
developer_security_group_reader_access   = true
budget_alert_amount                      = 32000 # See issue #2359

databricks_readers_group = {
  id   = "729028915538231"
  name = "SEC-G-Datahub-DevelopersAzure"
}
databricks_contributor_dataplane_group = {
  id   = "284159814927462"
  name = "SEC-A-Datahub-Dev-001-Contributor-Dataplane"
}
apim_address_prefixes             = ["10.143.102.0/28"]
privateendpoints_address_prefixes = ["10.143.96.0/22"]
vnetintegrations_address_prefixes = ["10.143.100.0/23"]
apim_next_hop_ip_address          = "10.142.96.196"
udr_firewall_next_hop_ip_address  = "10.142.65.196"
inbounddns_address_prefixes       = ["10.143.102.16/28"]
outbounddns_address_prefixes      = ["10.143.102.32/28"]
release_toggle_group_name         = "SEC-A-Datahub-Dev-001-Contributor-Dataplane"

# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/terraform.tfvars
apim_b2c_tenant_id                       = "20e7a6b4-86e0-4e7a-a34d-6dc5a75d1982"
virtual_network_resource_group_name      = "rg-network-online-preprod-we-001"
virtual_network_name                     = "vnet-datahub-online-preprod-we-001"
biztalk_hybrid_connection_hostname       = "datahub.preproduction.biztalk.energinet.local:443"
apim_url                                 = "https://preprod.b2b.datahub3.dk"
enable_audit_logs                        = false
pim_contributor_data_plane_group_name    = "SEC-A-Datahub-PreProd-001-Contributor-Dataplane"
pim_contributor_control_plane_group_name = "SEC-A-Datahub-PreProd-001-Contributor-Controlplane"
pim_reader_group_name                    = "SEC-A-Datahub-PreProd-001-Reader"
alert_email_address                      = "26a83fcc.energinet.onmicrosoft.com@emea.teams.ms"
budget_alert_amount                      = 60000 # See issue #2359
databricks_readers_group = {
  id   = "254255312660677"
  name = "SEC-A-Datahub-PreProd-001-Reader"
}
databricks_contributor_dataplane_group = {
  id   = "654194577919190"
  name = "SEC-A-Datahub-PreProd-001-Contributor-Dataplane"
}
apim_address_prefixes             = ["10.141.102.0/28"]
privateendpoints_address_prefixes = ["10.141.96.0/22"]
vnetintegrations_address_prefixes = ["10.141.100.0/23"]
apim_next_hop_ip_address          = "10.140.96.196"
udr_firewall_next_hop_ip_address  = "10.140.65.196"
inbounddns_address_prefixes       = ["10.141.102.16/28"]
outbounddns_address_prefixes      = ["10.141.102.32/28"]
enable_autoscale_apim             = true
app_configuration_sku             = "premium"
release_toggle_group_name         = "SEC-G-DataHub-Release-Toggle-Managers-preprod"

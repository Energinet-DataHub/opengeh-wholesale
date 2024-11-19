# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/terraform.tfvars
apim_b2c_tenant_id                         = "fde5680b-ddcb-4777-8c1b-52df2d612afb"
virtual_network_resource_group_name        = "rg-network-online-preprod-we-002"
virtual_network_name                       = "vnet-datahub-online-preprod-we-002"
apim_address_space                         = "10.141.7.128/28"
private_endpoint_address_space             = "10.141.6.0/25"
vnet_integration_address_space             = "10.141.7.0/25"
biztalk_hybrid_connection_hostname         = "datahub.preproduction.biztalk.energinet.local:443"
apim_url                                   = "https://preprod002.b2b.datahub3.dk"
enable_audit_logs                          = false
platform_security_group_contributor_access = true
platform_security_group_reader_access      = true
budget_alert_amount                        = 35000 # Copy of test_002
databricks_readers_group = {
  id   = "729028915538231"
  name = "SEC-G-Datahub-DevelopersAzure"
}
databricks_contributor_dataplane_group = {
  id   = "729028915538231"
  name = "SEC-G-Datahub-DevelopersAzure"
}
apim_address_prefixes             = ["10.141.118.0/28"]
privateendpoints_address_prefixes = ["10.141.112.0/22"]
vnetintegrations_address_prefixes = ["10.141.116.0/23"]
apim_next_hop_ip_address          = "10.140.96.196"
udr_firewall_next_hop_ip_address  = "10.140.65.196"

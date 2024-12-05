# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/platform.auto.tfvars
databricks_vnet_address_space              = "10.142.104.0/22"
databricks_private_subnet_address_prefix   = "10.142.104.0/24"
databricks_public_subnet_address_prefix    = "10.142.105.0/24"
developer_security_group_name              = "SEC-G-Datahub-DevelopersAzure"
platform_security_group_contributor_access = true
platform_security_group_reader_access      = true
datahub_bi_endpoint_enabled                = true
databricks_readers_group = {
  id   = "729028915538231"
  name = "SEC-G-Datahub-DevelopersAzure"
}
# Uses security group intended for test-001, to mimic setup in 001-swinlane
databricks_contributor_dataplane_group = {
  id   = "318888432738283"
  name = "SEC-A-Datahub-Test-001-Contributor-Dataplane"
}
enable_audit_logs = false

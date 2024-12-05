# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/platform.auto.tfvars
databricks_vnet_address_space               = "10.140.100.0/22"
databricks_private_subnet_address_prefix    = "10.140.100.0/24"
databricks_public_subnet_address_prefix     = "10.140.101.0/24"
enable_audit_logs                           = false
developer_security_group_name               = "SEC-G-Datahub-DevelopersAzure"
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

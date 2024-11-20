# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/platform.auto.tfvars
databricks_vnet_address_space                      = "10.140.100.0/22"
databricks_private_subnet_address_prefix           = "10.140.100.0/24"
databricks_public_subnet_address_prefix            = "10.140.101.0/24"
databricks_private_endpoints_subnet_address_prefix = "10.140.102.0/24"
pim_contributor_data_plane_group_name              = "SEC-A-Datahub-Dev-001-Contributor-Dataplane"
pim_contributor_control_plane_group_name           = "SEC-A-Datahub-Dev-001-Contributor-Controlplane"
developer_security_group_reader_access             = true
enable_audit_logs                                  = false
developer_security_group_name                      = "SEC-G-Datahub-DevelopersAzure"
databricks_readers_group = {
  id   = "729028915538231"
  name = "SEC-G-Datahub-DevelopersAzure"
}
databricks_contributor_dataplane_group = {
  id   = "284159814927462"
  name = "SEC-A-Datahub-Dev-001-Contributor-Dataplane"
}

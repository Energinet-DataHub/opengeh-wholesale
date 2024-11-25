# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/platform.auto.tfvars
databricks_vnet_address_space                      = "10.140.100.0/22"
databricks_private_subnet_address_prefix           = "10.140.100.0/24"
databricks_public_subnet_address_prefix            = "10.140.101.0/24"
databricks_private_endpoints_subnet_address_prefix = "10.140.102.0/24"
pim_contributor_data_plane_group_name              = "SEC-A-Datahub-Dev-003-Contributor-Dataplane"
pim_contributor_control_plane_group_name           = "SEC-A-Datahub-Dev-003-Contributor-Controlplane"
pim_reader_group_name                              = "SEC-A-Datahub-Dev-003-Reader"
enable_audit_logs                                  = false
databricks_readers_group = {
  id   = "629664400987703"
  name = "SEC-A-Datahub-Dev-003-Reader"
}
databricks_contributor_dataplane_group = {
  id   = "1105450622987696"
  name = "SEC-A-Datahub-Dev-003-Contributor-Dataplane"
}

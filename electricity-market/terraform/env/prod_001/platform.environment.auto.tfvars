# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/platform.auto.tfvars
pim_contributor_data_plane_group_name    = "SEC-A-Datahub-Prod-001-Contributor-Dataplane"
pim_contributor_control_plane_group_name = "SEC-A-Datahub-Prod-001-Contributor-Controlplane"
pim_reader_group_name                    = "SEC-A-Datahub-Prod-001-Reader"
enable_audit_logs                        = true
databricks_vnet_address_space            = "10.146.116.0/22"
databricks_private_subnet_address_prefix = "10.146.116.0/24"
databricks_public_subnet_address_prefix  = "10.146.117.0/24"
databricks_readers_group = {
  id   = "726131153567802"
  name = "SEC-A-Datahub-Prod-001-Reader"
}
databricks_contributor_dataplane_group = {
  id   = "504707241967571"
  name = "SEC-A-Datahub-Prod-001-Contributor-Dataplane"
}

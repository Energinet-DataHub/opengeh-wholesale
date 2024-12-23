# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/platform.auto.tfvars
pim_contributor_data_plane_group_name    = "SEC-A-Datahub-Test-001-Contributor-Dataplane"
pim_contributor_control_plane_group_name = "SEC-A-Datahub-Test-001-Contributor-Controlplane"
developer_security_group_reader_access   = true
enable_audit_logs                        = false
developer_security_group_name            = "SEC-G-Datahub-DevelopersAzure"
databricks_readers_group = {
  id   = "729028915538231"
  name = "SEC-G-Datahub-DevelopersAzure"
}
databricks_contributor_dataplane_group = {
  id   = "318888432738283"
  name = "SEC-A-Datahub-Test-001-Contributor-Dataplane"
}

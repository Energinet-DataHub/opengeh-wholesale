resource "databricks_permissions" "cluster_usage" {
  provider   = databricks.dbw
  cluster_id = databricks_cluster.shared_all_purpose.id

  access_control {
    group_name       = "SEC-A-GreenForce-DevelopmentTeamAzure"
    permission_level = "CAN_MANAGE"
  }
}

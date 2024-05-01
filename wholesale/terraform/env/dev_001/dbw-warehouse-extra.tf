resource "databricks_permissions" "databricks_sql_endpoint" {
  provider        = databricks.dbw
  sql_endpoint_id = databricks_sql_endpoint.this.id

  access_control {
    group_name       = "SEC-A-GreenForce-DevelopmentTeamAzure"
    permission_level = "CAN_MANAGE"
  }
}

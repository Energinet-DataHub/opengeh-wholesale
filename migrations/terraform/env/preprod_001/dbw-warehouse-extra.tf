resource "databricks_permissions" "endpoint_permissions" {
  provider        = databricks.dbw
  sql_endpoint_id = databricks_sql_endpoint.migration_sql_endpoint.id

  access_control {
    group_name       = "SEC-A-GreenForce-DevelopmentTeamAzure"
    permission_level = "CAN_MANAGE"
  }

}

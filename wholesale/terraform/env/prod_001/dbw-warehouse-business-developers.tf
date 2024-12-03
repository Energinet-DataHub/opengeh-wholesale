# SQL warehouse to host Business developers SQL statements
resource "databricks_sql_endpoint" "business_developers_access" {
  provider             = databricks.dbw
  name                 = "Business Developers SQL Endpoint"
  cluster_size         = "Small"
  max_num_clusters     = 3
  auto_stop_mins       = 60
  warehouse_type       = "PRO"
  spot_instance_policy = "RELIABILITY_OPTIMIZED"
  channel {
    name = "CHANNEL_NAME_CURRENT"
  }
}

resource "databricks_permissions" "business_developers_access" {
  provider        = databricks.dbw
  sql_endpoint_id = databricks_sql_endpoint.this.id

  access_control {
    group_name       = "SEC-G-DataHub-BusinessDevelopers"
    permission_level = "CAN_MONITOR"
  }
  depends_on = [module.dbw]
}

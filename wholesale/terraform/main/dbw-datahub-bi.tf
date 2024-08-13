# SQL warehouse for SAP BI
resource "databricks_sql_endpoint" "datahub_bi" {
  count            = var.datahub_bi_endpoint_enabled ? 1 : 0

  provider         = databricks.dbw
  name             = "Datahub BI SQL Endpoint"
  cluster_size     = "Small"
  max_num_clusters = 10
  auto_stop_mins   = 60
  warehouse_type   = "PRO"
}

resource "databricks_permissions" "databricks_permissions_datahub_bi_endpoint" {
  count            = var.datahub_bi_endpoint_enabled ? 1 : 0

  provider        = databricks.dbw
  sql_endpoint_id = databricks_sql_endpoint.datahub_bi[0].id

  access_control {
    group_name       = "SEC-G-Datahub-DevelopersAzure"
    permission_level = "CAN_MONITOR"
  }

  depends_on = [module.dbw, null_resource.scim_developers]
}

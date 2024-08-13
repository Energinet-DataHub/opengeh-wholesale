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

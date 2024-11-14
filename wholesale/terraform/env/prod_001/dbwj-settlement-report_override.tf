resource "databricks_instance_pool" "settlement_report_cluster_pool" {
  min_idle_instances = 2
  max_capacity = 36
}

resource "databricks_instance_pool" "settlement_report_cluster_pool" {
    min_idle_instances = 4
    idle_instance_autotermination_minutes = 60
}

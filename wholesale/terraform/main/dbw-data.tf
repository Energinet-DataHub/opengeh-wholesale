data "databricks_spark_version" "latest_lts" {
  provider          = databricks.dbw
  long_term_support = true
  depends_on        = [module.dbw]
}

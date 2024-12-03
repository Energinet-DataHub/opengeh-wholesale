# 1109792873721454 is SEC-G-DataHub-BusinessDevelopers
module "dbw" {
  scim_databrick_group_ids = [
    var.databricks_readers_group.id,
    var.databricks_contributor_dataplane_group.id,
    "1109792873721454"
  ]
}

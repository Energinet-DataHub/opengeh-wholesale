module "mssqldb_revision_log" {
  security_groups = local.pim_security_group_rules_001
  max_size_gb     = 500
}

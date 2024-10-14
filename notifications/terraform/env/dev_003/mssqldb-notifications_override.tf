module "mssqldb_notifications" {
  security_groups = local.pim_security_group_rules_001
}

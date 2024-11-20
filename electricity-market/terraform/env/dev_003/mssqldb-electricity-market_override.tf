module "mssqldb_electricity_market" {
  security_groups = local.pim_security_group_rules_001
}

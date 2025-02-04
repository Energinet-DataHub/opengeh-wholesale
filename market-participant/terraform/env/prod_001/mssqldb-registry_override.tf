module "mssqldb_market_participant" {
  security_groups = local.pim_security_group_rules_001
}

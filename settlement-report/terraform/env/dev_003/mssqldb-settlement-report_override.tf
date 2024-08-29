module "mssqldb_settlement_report" {
  security_groups = local.pim_security_group_rules_001
}

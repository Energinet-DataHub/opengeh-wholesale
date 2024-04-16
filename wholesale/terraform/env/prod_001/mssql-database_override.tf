module "mssqldb_wholesale" {
  security_groups = local.pim_security_group_rules_001
}

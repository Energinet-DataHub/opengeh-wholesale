module "mssqldb_process_manager" {
  security_groups = concat(local.pim_security_group_rules_001, local.developer_security_group_rules_001_dev_test)
}

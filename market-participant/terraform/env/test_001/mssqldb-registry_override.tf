module "mssqldb_market_participant" {
  max_size_gb      = 150
  sku_name        = "GP_S_Gen5_8"
  security_groups  = concat(local.pim_security_group_rules_001, local.developer_security_group_rules_001_dev_test)
}

module "mssqldb_esett_exchange" {
  prevent_deletion = true
  security_groups  = local.pim_security_group_rules_001
}

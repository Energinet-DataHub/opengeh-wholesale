module "mssqldb_process_manager" {
  security_groups = local.pim_security_group_rules_001
  # This needs to be specified since it's specified on the elastic pool in shared
  zone_redundant = true
}

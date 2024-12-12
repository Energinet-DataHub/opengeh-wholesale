module "mssqldb_process_manager" {
  security_groups = local.pim_security_group_rules_001
  # Enabling zone redundancy for the database
  zone_redundant = true
}

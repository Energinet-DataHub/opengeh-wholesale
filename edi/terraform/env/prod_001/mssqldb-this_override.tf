module "mssqldb_edi" {
  # Max size of the database in GB
  max_size_gb = 128

  # Enabling zone redundancy for the database, as it is not in the elastic pool
  zone_redundant  = true
  security_groups = local.pim_security_group_rules_001
}

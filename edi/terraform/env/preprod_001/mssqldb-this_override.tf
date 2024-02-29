module "mssqldb_edi" {
  # Enabling zone redundancy for the database, as it is not in the elastic pool
  zone_redundant = true
}

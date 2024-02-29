module "mssql_data_additional" {
  elastic_pool_sku = {
    name     = "PremiumPool"
    tier     = "Premium"
    capacity = 250
  }

  elastic_pool_per_database_settings = {
    min_capacity = 0
    max_capacity = 125
  }
}

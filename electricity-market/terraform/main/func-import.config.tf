locals {
  func_import = {
    app_settings = {
      "Database:ConnectionString" = local.MS_MARKPART_DB_CONNECTION_STRING

      "Databricks:WorkspaceUrl"   = "https://localhost"
      "Databricks:WorkspaceToken" = "tbd"
      "Databricks:WarehouseId"    = "tbd"
    }
  }
}

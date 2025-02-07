locals {
  func_mp_data_api = {
    app_settings = {
      "Database:ConnectionString" = local.MS_MARKPART_DB_CONNECTION_STRING
    }
  }
}

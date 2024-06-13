locals {
  app_reader_api = {
    app_settings = {
      "DatabaseSettings__ConnectionString" = local.MS_REVISION_LOG_CONNECTION_STRING
    }
  }
}

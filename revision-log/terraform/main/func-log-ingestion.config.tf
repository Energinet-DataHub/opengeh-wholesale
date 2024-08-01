locals {
  func_log_ingestion = {
    app_settings = {
      "DatabaseOptions:ConnectionString" = local.MS_REVISION_LOG_CONNECTION_STRING
    }
  }
}

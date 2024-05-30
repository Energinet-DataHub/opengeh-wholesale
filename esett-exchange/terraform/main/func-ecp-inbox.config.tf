locals {
  func_entrypoint_ecp_inbox = {
    app_settings = {
      "DatabaseSettings:ConnectionString"      = local.MS_ESETT_EXCHANGE_CONNECTION_STRING
      "BlobStorageSettings:AccountUri"         = local.ESETT_DOCUMENT_STORAGE_ACCOUNT_URI
      "BlobStorageSettings:ContainerName"      = local.ESETT_DOCUMENT_STORAGE_CONTAINER_NAME
      "BlobStorageSettings:ErrorContainerName" = local.ESETT_ERROR_DOCUMENT_STORAGE_CONTAINER_NAME
    }
  }
}

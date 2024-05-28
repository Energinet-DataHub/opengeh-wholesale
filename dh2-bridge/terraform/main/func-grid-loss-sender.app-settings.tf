locals {
  default_sender_app_settings = {
    WEBSITE_LOAD_CERTIFICATES           = local.DH2BRIDGE_CERTIFICATE_THUMBPRINT
    "BlobStorageSettings:AccountUri"    = local.DH2_BRIDGE_DOCUMENT_STORAGE_ACCOUNT_URI
    "BlobStorageSettings:ContainerName" = local.DH2_BRIDGE_DOCUMENT_STORAGE_CONTAINER_NAME
    "DatabaseSettings:ConnectionString" = local.MS_DH2_BRIDGE_CONNECTION_STRING
    "DataHub2Settings:DataHub2Endpoint" = local.DH2_ENDPOINT
  }
}

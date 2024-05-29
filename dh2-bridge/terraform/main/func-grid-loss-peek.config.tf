locals {
  entrypoint_grid_loss_peek = {
    app_settings = {
      WEBSITE_LOAD_CERTIFICATES                               = local.DH2BRIDGE_CERTIFICATE_THUMBPRINT
      "EmailNotificationConfig:SendGridApiKey"                = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_sendgrid_api_key.name})"
      "EmailNotificationConfig:RejectedNotificationToEmail"   = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_sendgrid_to_email.name})"
      "EmailNotificationConfig:RejectedNotificationFromEmail" = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_sendgrid_from_email.name})"
      "BlobStorageSettings:AccountUri"                        = local.DH2_BRIDGE_DOCUMENT_STORAGE_ACCOUNT_URI
      "BlobStorageSettings:ContainerName"                     = local.DH2_BRIDGE_DOCUMENT_STORAGE_CONTAINER_NAME
      "DatabaseSettings:ConnectionString"                     = local.MS_DH2_BRIDGE_CONNECTION_STRING
      "DataHub2Settings:DataHub2Endpoint"                     = var.dh2_endpoint != null ? var.dh2_endpoint : "https://${module.app_dh2_placeholder.default_hostname}"
    }
  }
}

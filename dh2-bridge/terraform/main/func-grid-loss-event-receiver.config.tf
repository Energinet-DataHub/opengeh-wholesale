locals {
  entrypoint_grid_loss_event_receiver = {
    app_settings = {
      # Authentication
      "UserAuthentication__MitIdExternalMetadataAddress"      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=mitid-frontend-open-id-url)"
      "UserAuthentication__ExternalMetadataAddress"           = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=frontend-open-id-url)"
      "UserAuthentication__InternalMetadataAddress"           = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=api-backend-open-id-url)"
      "UserAuthentication__BackendBffAppId"                   = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-bff-app-id)"

      "BlobStorageSettings:AccountUri"                        = local.DH2_BRIDGE_DOCUMENT_STORAGE_ACCOUNT_URI
      "BlobStorageSettings:ContainerName"                     = local.DH2_BRIDGE_DOCUMENT_STORAGE_CONTAINER_NAME
      "DatabaseSettings:ConnectionString"                     = local.MS_DH2_BRIDGE_CONNECTION_STRING

      "IntegrationEvents:TopicName"                           = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"
      "IntegrationEvents:SubscriptionName"                    = module.sbtsub_dh2_bridge_event_listener.name
      "ServiceBus:FullyQualifiedNamespace"                    = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-namespace-endpoint)"

      "ConverterSettings:RecipientPartyGLN"                   = var.dh2_bridge_recipient_party_gln
      "ConverterSettings:SenderPartyGLN"                      = var.dh2_bridge_sender_party_gln
      "RevisionLogOptions:ApiAddress"                         = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=func-log-ingestion-api-url)"

      # Databricks
      "DatabricksOptions:WorkspaceToken"                      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-wholesale-workspace-token)"
      "DatabricksOptions:WorkspaceUrl"                        = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-wholesale-workspace-url)"
      "DatabricksOptions:WarehouseId"                         = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-wholesale-warehouse-id)"
      "DatabricksSchemaSettings:CatalogName"                  = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=shared-unity-catalog-name)"
    }
  }
}

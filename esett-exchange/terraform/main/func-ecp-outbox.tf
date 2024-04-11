module "func_entrypoint_ecp_outbox" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v13"

  name                                   = "ecp-outbox"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                    = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  always_on                              = true
  health_check_path                      = "/api/monitor/ready"
  health_check_alert = {
    action_group_id = data.azurerm_key_vault_secret.primary_action_group_id.value
    enabled         = var.enable_health_check_alerts
  }
  dotnet_framework_version    = "v8.0"
  use_dotnet_isolated_runtime = true
  role_assignments = [
    {
      resource_id          = module.storage_esett_documents.id
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      resource_id          = data.azurerm_key_vault.kv_shared_resources.id
      role_definition_name = "Key Vault Secrets User"
    }
  ]
  app_settings = local.default_outbox_app_settings
}

locals {
  default_outbox_app_settings = {
    WEBSITE_LOAD_CERTIFICATES = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=cert-esett-biztalk-thumbprint)"

    "DatabaseSettings:ConnectionString" = local.MS_ESETT_EXCHANGE_CONNECTION_STRING
    "BlobStorageSettings:AccountUri"    = local.ESETT_DOCUMENT_STORAGE_ACCOUNT_URI
    "BlobStorageSettings:ContainerName" = local.ESETT_DOCUMENT_STORAGE_CONTAINER_NAME

    "EcpSettings:SenderCode"              = local.BIZ_TALK_SENDER_CODE
    "EcpSettings:ReceiverCode"            = local.BIZ_TALK_RECEIVER_CODE
    "EcpSettings:BiztalkRootUrl"          = "https://${var.biztalk_hybrid_connection_hostname}"
    "EcpSettings:BizTalkEndPoint"         = local.BIZ_TALK_BIZ_TALK_END_POINT
    "EcpSettings:BusinessTypeConsumption" = local.BIZ_TALK_BUSINESS_TYPE_CONSUMPTION
    "EcpSettings:BusinessTypeProduction"  = local.BIZ_TALK_BUSINESS_TYPE_PRODUCTION
    "EcpSettings:BusinessTypeExchange"    = local.BIZ_TALK_BUSINESS_TYPE_EXCHANGE
    "EcpSettings:DisableBizTalkBackOff"   = var.disable_biztalk_backoff

    "FeatureManagement__DisableBizTalkConnectionCheck" = var.disable_biztalk_connection_check
  }
}

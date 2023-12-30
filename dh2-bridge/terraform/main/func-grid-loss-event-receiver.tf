module "func_entrypoint_grid_loss_event_receiver" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v13"

  name                                     = "grid-loss-event-receiver"
  project_name                             = var.domain_name_short
  environment_short                        = var.environment_short
  environment_instance                     = var.environment_instance
  resource_group_name                      = azurerm_resource_group.this.name
  location                                 = azurerm_resource_group.this.location
  vnet_integration_subnet_id               = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id               = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                      = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key = data.azurerm_key_vault_secret.appi_shared_instrumentation_key.value
  ip_restrictions                          = var.ip_restrictions
  scm_ip_restrictions                      = var.ip_restrictions
  always_on                                = true
  health_check_path                        = "/api/monitor/ready"
  health_check_alert = {
    action_group_id = data.azurerm_key_vault_secret.primary_action_group_id.value
    enabled         = var.enable_health_check_alerts
  }
  dotnet_framework_version    = "v7.0"
  use_dotnet_isolated_runtime = true
  role_assignments = [
    {
      resource_id          = module.storage_dh2_bridge_documents.id
      role_definition_name = "Storage Blob Data Contributor"
    }
  ]
  app_settings = {
    "BlobStorageSettings:AccountUri"                        = local.DH2_BRIDGE_DOCUMENT_STORAGE_ACCOUNT_URI
    "BlobStorageSettings:ContainerName"                     = local.DH2_BRIDGE_DOCUMENT_STORAGE_CONTAINER_NAME
    "DatabaseSettings:ConnectionString"                     = local.MS_DH2_BRIDGE_CONNECTION_STRING
    "ConsumeServiceBusSettings:ConnectionString"            = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-listen-connection-string)"
    "ConsumeServiceBusSettings:HealthCheckConnectionString" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-manage-connection-string)"
    "ConsumeServiceBusSettings:SharedIntegrationEventTopic" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"
    "ConsumeServiceBusSettings:GridLossSubscription"        = module.sbtsub_dh2_bridge_event_listener.name
    "ConverterSettings:RecipientPartyGLN"                   = local.DH2_BRIDGE_RECIPIENT_PARTY_GLN
    "ConverterSettings:SenderPartyGLN"                      = local.DH2_BRIDGE_SENDER_PARTY_GLN
  }
}

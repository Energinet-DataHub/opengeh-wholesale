module "func_entrypoint_peek" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v12"

  name                                     = "peek"
  project_name                             = var.domain_name_short
  environment_short                        = var.environment_short
  environment_instance                     = var.environment_instance
  resource_group_name                      = azurerm_resource_group.this.name
  location                                 = azurerm_resource_group.this.location
  vnet_integration_subnet_id               = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id               = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                      = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key = data.azurerm_key_vault_secret.appi_shared_instrumentation_key.value
  ip_restriction_allow_ip_range            = var.hosted_deployagent_public_ip_range
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
      resource_id          = module.storage_esett_documents.id
      role_definition_name = "Storage Blob Data Contributor"
    }
  ]
  app_settings = {
    "PublishServiceBusSettings:ConnectionString"            = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-send-connection-string)"
    "PublishServiceBusSettings:HealthCheckConnectionString" = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-manage-connection-string)"
    "PublishServiceBusSettings:SharedIntegrationEventTopic" = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbt-shres-integrationevent-received-name)"
    "DataHub2Settings:DataHub2Endpoint"                     = "https://${module.app_dh2_placeholder.default_hostname}"
    "DataHub2Settings:CertificateSubjectName"               = "tbd-dh2-cert-subject-name"
    "BlobStorageSettings:AccountUri"                        = local.ESETT_DOCUMENT_STORAGE_ACCOUNT_URI
    "BlobStorageSettings:ContainerName"                     = local.ESETT_DOCUMENT_STORAGE_CONTAINER_NAME
  }
}

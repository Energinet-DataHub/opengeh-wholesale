module "func_entrypoint_certificate_synchronization" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v13"

  name                                     = "certificatesynchronization"
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

  app_settings = {
    CERTIFICATES_KEY_VAULT = module.kv_dh2_certificates.vault_uri
    APIM_SERVICE_NAME      = data.azurerm_key_vault_secret.apim_instance_id.value

    APIM_TENANT_ID        = data.azurerm_subscription.this.tenant_id
    APIM_SP_CLIENT_ID     = azuread_application.app_market_participant.application_id
    APIM_SP_CLIENT_SECRET = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_app_market_participant_password.name})"
  }
}

module "func_certificatesynchronization" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v14"

  name                                   = "certificatesynchronization"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                    = module.webapp_service_plan.id
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  health_check_path                      = "/api/monitor/ready"
  always_on                              = true
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  health_check_alert = {
    action_group_id = data.azurerm_key_vault_secret.primary_action_group_id.value
    enabled         = var.enable_health_check_alerts
  }
  dotnet_framework_version    = "v8.0"
  use_dotnet_isolated_runtime = true
  app_settings                = local.default_certificatesynchronization_app_settings

  role_assignments = [
    {
      resource_id          = module.kv_internal.id
      role_definition_name = "Key Vault Secrets User"
    },
    {
      resource_id          = module.kv_dh2_certificates.id
      role_definition_name = "Key Vault Secrets Officer"
    }
  ]
}

locals {
  default_certificatesynchronization_app_settings = {
    "CertificateSynchronization:CertificatesKeyVault" = module.kv_dh2_certificates.vault_uri
    "CertificateSynchronization:ApimServiceName"      = data.azurerm_key_vault_secret.apim_instance_id.value
    "CertificateSynchronization:ApimTenantId"         = data.azurerm_subscription.this.tenant_id
    "CertificateSynchronization:ApimSpClientId"       = azuread_application.app_market_participant.application_id
    "CertificateSynchronization:ApimSpClientSecret"   = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_app_market_participant_password.name})"
  }
}

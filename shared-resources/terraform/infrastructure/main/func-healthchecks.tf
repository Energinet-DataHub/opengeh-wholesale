module "func_healthchecks" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v13"

  name                                      = "healthchecks"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  vnet_integration_subnet_id                = module.kvs_snet_vnet_integration_id.value  # For reasons beyond me, 'module.snet_vnet_integration.id' fails with an "Unsupported attribute" error as the module returns a list of objects ???
  private_endpoint_subnet_id                = module.kvs_snet_private_endpoints_id.value # See above
  app_service_plan_id                       = module.plan_services.id
  application_insights_instrumentation_key  = module.appi_shared.instrumentation_key
  ip_restriction_allow_ip_range             = var.hosted_deployagent_public_ip_range
  dotnet_framework_version                  = "v7.0"
  app_settings = {
    SHARED_KEYVAULT_NAME          = "${module.kv_shared.name}"
    SHARED_DATALAKE_NAME          = "@Microsoft.KeyVault(VaultName=${module.kv_shared.name};SecretName=st-data-lake-name)"
    SERVICEBUS_CONNECTION_STRING  = "@Microsoft.KeyVault(VaultName=${module.kv_shared.name};SecretName=sb-domain-relay-manage-connection-string)"
    SERVICEBUS_TOPIC_NAME         = "@Microsoft.KeyVault(VaultName=${module.kv_shared.name};SecretName=sbt-shres-integrationevent-received-name)"
  }
}

// Access policy to allow checking access to Shared Resources keyvault
module "kv_shared_access_policy_func_entrypoint_marketparticipant" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-access-policy?ref=v13"

  key_vault_id = module.kv_shared.id
  app_identity = module.func_healthchecks.identity.0
}

resource "azurerm_role_assignment" "func_healthchecks_read_access_to_stdatalake" {
  scope                = module.st_data_lake.id
  role_definition_name = "Reader"
  principal_id         = module.func_healthchecks.identity.0.principal_id
}

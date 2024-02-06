module "func_githubapi" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v13"

  name                                   = "githubapi"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                    = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  ip_restriction_allow_ip_range          = var.hosted_deployagent_public_ip_range
  dotnet_framework_version               = "v8.0"
  app_settings = {
    CONNECTION_STRING_DATABASE = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb.name};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
    SHARED_KEYVAULT_NAME       = "${var.shared_resources_keyvault_name}"
  }
}

// Access policy to allow checking access to Shared Resources keyvault
module "kv_shared_access_policy_func_entrypoint_marketparticipant" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-access-policy?ref=v13"

  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
  app_identity = module.func_githubapi.identity.0
}

// Access policy to allow checking access to Shared Resources storage account
data "azurerm_storage_account" "st_data_lake" {
  name                = data.azurerm_key_vault_secret.st_data_lake_name.value
  resource_group_name = var.shared_resources_resource_group_name
}

resource "azurerm_role_assignment" "func_githubapi_read_access_to_stdatalake" {
  scope                = data.azurerm_storage_account.st_data_lake.id
  role_definition_name = "Reader"
  principal_id         = module.func_githubapi.identity.0.principal_id
}

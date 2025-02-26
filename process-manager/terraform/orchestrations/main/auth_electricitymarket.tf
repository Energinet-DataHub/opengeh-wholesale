resource "azuread_app_role_assignment" "processmanager_orchestrations_electricitymarket_role_assignment" {
  app_role_id         = "00000000-0000-0000-0000-000000000000" // default role
  resource_object_id  = data.azurerm_key_vault_secret.electricitymarket_sp_object_id.value
  principal_object_id = module.func_orchestrations.identity[0].principal_id
}

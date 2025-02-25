resource "azuread_app_role_assignment" "bff_processmanager_role_assignment" {
  app_role_id         = "00000000-0000-0000-0000-000000000000" // default role
  resource_object_id  = data.azurerm_key_vault_secret.processmanager_sp_object_id.value
  principal_object_id = module.backend_for_frontend.identity[0].principal_id
}

resource "azuread_app_role_assignment" "bff_slot_processmanager_role_assignment" {
  app_role_id         = "00000000-0000-0000-0000-000000000000" // default role
  resource_object_id  = data.azurerm_key_vault_secret.processmanager_sp_object_id.value
  principal_object_id = module.backend_for_frontend.slot_identity[0].principal_id
}

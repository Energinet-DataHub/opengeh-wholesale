# resource "azurerm_role_assignment" "spn_market_participant_api_managment_contributor" {
#   scope                = data.azurerm_key_vault_secret.apim_instance_id.value
#   role_definition_name = "API Management Service Contributor"
#   principal_id         = azuread_service_principal.spn_market_participant.object_id
# }

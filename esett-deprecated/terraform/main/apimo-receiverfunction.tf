module "apimao_receiverfunction" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api-operation?ref=v13"

  operation_id            = "receiverfunction"
  api_management_api_name = module.apim_biztalkreceiver.name
  resource_group_name     = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_management_name     = data.azurerm_key_vault_secret.apim_instance_name.value
  display_name            = "Receive messages from Biztalk"
  method                  = "POST"
  url_template            = "/api/ReceiverFunction"
}

module "apim_biztalkreceiver" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api?ref=v13"

  count                      = 1
  name                       = "biztalkreceiver"
  project_name               = var.domain_name_short
  api_management_name        = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name        = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  display_name               = "Biztalk Receiver function"
  authorization_server_name  = data.azurerm_key_vault_secret.apim_oauth_server_name.value
  apim_logger_id             = data.azurerm_key_vault_secret.apim_logger_id.value
  logger_sampling_percentage = 100.0
  logger_verbosity           = "verbose"
  path                       = "biztalkreceiver"
  backend_service_url        = "https://${module.func_biztalkreceiver.default_hostname}"
  policies = [
    {
      xml_content = <<XML
                <policies>
                    <inbound>
                        <base />
                        <set-backend-service backend-id="biztalkreceiver" />
                    </inbound>
                    <backend>
                        <base />
                    </backend>
                    <outbound>
                        <base />
                    </outbound>
                    <on-error>
                        <base />
                    </on-error>
                </policies>
            XML
    }
  ]
}

resource "azurerm_api_management_backend" "biztalkreceiver" {
  count               = 1

  name                = "biztalkreceiver"
  resource_group_name = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_management_name = data.azurerm_key_vault_secret.apim_instance_name.value
  protocol            = "http"
  url                 = "https://${module.func_biztalkreceiver.default_hostname}"
}

module "apimao_receiverfunction" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api-operation?ref=v13"

  count                   = 1
  operation_id            = "receiverfunction"
  api_management_api_name = module.apim_biztalkreceiver[0].name
  resource_group_name     = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_management_name     = data.azurerm_key_vault_secret.apim_instance_name.value
  display_name            = "Receive messages from Biztalk"
  method                  = "POST"
  url_template            = "/api/ReceiverFunction"
}

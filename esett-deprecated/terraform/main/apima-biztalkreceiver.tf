module "apim_biztalkreceiver" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api?ref=v13"

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

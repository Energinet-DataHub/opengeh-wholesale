module "apimao-b2c-archivedmessage-search" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api-operation?ref=v13"

  operation_id            = "b2c-archivedmessage-search"
  api_management_api_name = module.apima_b2c.name
  resource_group_name     = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_management_name     = data.azurerm_key_vault_secret.apim_instance_name.value
  display_name            = "EDI: Archived Message Search"
  method                  = "POST"
  url_template            = "/v1.0/ArchivedMessageSearch"
  policies = [
    {
      xml_content = <<XML
        <policies>
          <inbound>
            <base />
            <set-backend-service backend-id="${azurerm_api_management_backend.edi-b2c.name}" />
            <rewrite-uri template="/ArchivedMessageSearch" />
          </inbound>
        </policies>
      XML
    }
  ]
}

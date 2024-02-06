module "apimao-b2c-archivedmessage-getdocument" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api-operation?ref=v13"

  operation_id            = "b2c-archivedmessage-getdocument"
  api_management_api_name = module.apima_b2c.name
  resource_group_name     = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_management_name     = data.azurerm_key_vault_secret.apim_instance_name.value
  display_name            = "EDI: Archived Message Get Document"
  method                  = "POST"
  url_template            = "/v1.0/ArchivedMessageGetDocument"
  policies = [
    {
      xml_content = <<XML
        <policies>
          <inbound>
            <base />
            <set-backend-service backend-id="${azurerm_api_management_backend.edi-b2c.name}" />
            <rewrite-uri template="/ArchivedMessageGetDocument" />
          </inbound>
        </policies>
      XML
    }
  ]
}

module "apimao_cim_dequeue_message" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api-operation?ref=api-management-api-operation_5.0.0"

  operation_id            = "cim-dequeue-message"
  api_management_api_name = module.apima_b2b.name
  resource_group_name     = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_management_name     = data.azurerm_key_vault_secret.apim_instance_name.value
  display_name            = "EDI: CIM Dequeue Message"
  method                  = "DELETE"
  url_template            = "/v1.0/cim/dequeue/{message-id}"
  template_parameters = [
    {
      name     = "message-id"
      type     = "string"
      required = true
    }
  ]
  policies = [
    {
      xml_content = <<XML
        <policies>
          <inbound>
            <base />
            <validate-jwt header-name="Authorization" failed-validation-httpcode="403" failed-validation-error-message="Unauthorized to access this endpoint.">
                <openid-config url="https://login.microsoftonline.com/${data.azurerm_key_vault_secret.apim_b2c_tenant_id.value}/v2.0/.well-known/openid-configuration" />
            </validate-jwt>
            <set-backend-service backend-id="${azurerm_api_management_backend.edi.name}" />
            <rewrite-uri template="/api/dequeue/{message-id}" />
          </inbound>
        </policies>
      XML
    }
  ]
}

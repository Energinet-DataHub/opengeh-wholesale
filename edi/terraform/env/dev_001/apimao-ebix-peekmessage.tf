module "apimao_ebix_peekmessage" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api-operation?ref=v13"

  resource_group_name     = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_management_name     = data.azurerm_key_vault_secret.apim_instance_name.value
  api_management_api_name = module.apima_b2b_ebix.name

  operation_id            = "peek-message" // Taken from the imported WSDL, is this correct?
  method                  = "POST"
  display_name            = "Peek message - ebIX"
  url_template            = "/?soapAction=peekMessage"

  policies = [
    {
      xml_content = <<XML
        <policies>
          <inbound>
            <base />
            <set-backend-service backend-id="${azurerm_api_management_backend.edi.name}" />
            <rewrite-uri template="/api/peek/none" copy-unmatched-params="false" />
            <set-method>GET</set-method>
          </inbound>
          <backend>
            <base />
          </backend>
          <outbound>
            <base />
            <set-variable name="ResponseBody" value="@(context.Response.Body.As<string>().Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", ""))" />
            <choose>
              <when condition="@(context.Response.StatusCode == 204)">
                <set-status code="200" />
                <set-body template="liquid">
                  <soap-env:Envelope xmlns:soap-env="http://schemas.xmlsoap.org/soap/envelope/">
                    <soap-env:Body>
                      <PeekMessageResponse xmlns="urn:www:datahub:dk:b2b:v01" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                      </PeekMessageResponse>
                    </soap-env:Body>
                  </soap-env:Envelope>
                </set-body>
              </when>
              <when condition="@(context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)">
                <set-body template="liquid">
                  <soap-env:Envelope xmlns:soap-env="http://schemas.xmlsoap.org/soap/envelope/">
                    <soap-env:Body>
                      <PeekMessageResponse xmlns="urn:www:datahub:dk:b2b:v01" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                        {{context.Variables.ResponseBody}}
                      </PeekMessageResponse>
                    </soap-env:Body>
                  </soap-env:Envelope>
                </set-body>
              </when>
            </choose>
          </outbound>
          <on-error>
            <base />
          </on-error>
        </policies>
      XML
    }
  ]
}

module "apimao_ebix_dequemessage" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api-operation?ref=v13"

  resource_group_name     = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_management_name     = data.azurerm_key_vault_secret.apim_instance_name.value
  api_management_api_name = module.apima_b2b_ebix.name

  operation_id            = "deque-message" // Taken from the imported WSDL, is this correct?
  method                  = "POST"
  display_name            = "Deque message - ebIX"
  url_template            = "/?soapAction=dequeMessage"

  policies = [
    {
      xml_content = <<XML
        <policies>
          <inbound>
            <base />
            <set-variable name="messageId" value="@{
              var body = context.Request.Body.As<XElement>();
              return body.Element(XName.Get("MessageId", "urn:www:datahub:dk:b2b:v01")).Value;
            }" />
            <set-backend-service backend-id="${azurerm_api_management_backend.edi.name}" />
            <rewrite-uri template="@("/api/dequeue/" + context.Variables.GetValueOrDefault<string>("messageId"))" copy-unmatched-params="false" />
            <set-method>DELETE</set-method>
          </inbound>
          <backend>
            <base />
          </backend>
          <outbound>
            <base />
            <set-variable name="responseBody" value="@(context.Response.Body.As<string>().Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", ""))" />
            <choose>
              <when condition="@(context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)">
                <set-body template="liquid">
                  <soap-env:Envelope xmlns:soap-env="http://schemas.xmlsoap.org/soap/envelope/">
                    <soap-env:Body>
                      <DequeueMessageResponse xmlns:ns0="urn:www:datahub:dk:b2b:v01">
                        {{context.Variables["responseBody"]}}
                      </DequeueMessageResponse>
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

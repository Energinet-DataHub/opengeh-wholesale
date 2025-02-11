module "apimao_ebix_incomingmessage" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api-operation?ref=api-management-api-operation_7.0.0"

  operation_id = "incoming-message"
  resource_group_name     = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_management_name     = data.azurerm_key_vault_secret.apim_instance_name.value
  api_management_api_name = module.apima_b2b_ebix.name
  display_name            = "Incoming messages - ebIX"
  method                  = "POST"
  url_template            = "/?soapAction=sendMessage"

  policies = [
    {
      xml_content = <<XML
        <policies>
          <inbound>
            <base />
            <set-backend-service backend-id="${azurerm_api_management_backend.edi.name}" />
             <set-body>@(context.Request.Body.As<XElement>()
                .Descendants()
                .FirstOrDefault(a => a.Name.LocalName == "Payload")?
                .Elements().FirstOrDefault()?
                .ToString())</set-body>
            <rewrite-uri template="/api/incomingMessages/ebix" copy-unmatched-params="false" />
            <set-method>POST</set-method>
          </inbound>
          <backend>
            <base />
          </backend>
          <outbound>
            <base />
            <set-variable name="ResponseBody" value="@(context.Response.Body.As<string>(preserveContent: true).Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "").Replace("<Error>", "").Replace("</Error>", ""))" />
            <choose>
              <when condition="@(context.Response.StatusCode == 400)">
                <set-status code="200" />
                <set-body template="liquid">
                  <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
                    <soapenv:Body>
                      <soapenv:Fault>
                        {{context.Variables.ResponseBody}}
                      </soapenv:Fault>
                    </soapenv:Body>
                  </soapenv:Envelope>
                </set-body>
              </when>
              <when condition="@(context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)">
                <set-body template="liquid">
                  <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:urn="urn:www:datahub:dk:b2b:v01">
                    <soapenv:Header/>
                    <soapenv:Body>
                      <urn:SendMessageResponse>
                        <urn:MessageId>{{context.Variables.ResponseBody}}</urn:MessageId>
                      </urn:SendMessageResponse>
                    </soapenv:Body>
                  </soapenv:Envelope>
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

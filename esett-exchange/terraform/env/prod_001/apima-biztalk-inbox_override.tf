module "apim_biztalk_inbox" {
  policies = [
    {
      xml_content = <<XML
        <policies>
          <inbound>
            <base />
            <set-backend-service backend-id="${resource.azurerm_api_management_backend.biztalk_inbox_backend[0].name}" />
            <rewrite-uri template="/api/BizTalkReceiver" />
            <check-header name="X-Azure-ClientIP" failed-check-httpcode="403" failed-check-error-message="Blocked access" ignore-case="false">
              <value>194.239.2.103</value>
            </check-header>
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

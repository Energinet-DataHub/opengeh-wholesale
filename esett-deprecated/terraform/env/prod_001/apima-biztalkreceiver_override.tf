module "apim_biztalkreceiver" {
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

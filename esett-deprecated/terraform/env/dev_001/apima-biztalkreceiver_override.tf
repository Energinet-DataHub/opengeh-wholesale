module "apim_biztalkreceiver" {
  policies = [
    {
      xml_content = <<XML
                <policies>
                    <inbound>
                        <base />
                        <set-backend-service backend-id="biztalkreceiver" />
                        <ip-filter action="allow">
                            <address>10.154.7.149</address>
                            <address>10.178.7.149</address>
                            <address>10.154.7.150</address>
                        </ip-filter>
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

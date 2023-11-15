module "apima_b2b_ebix" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api?ref=v13"

  name                       = "b2b-ebix"
  project_name               = var.domain_name_short
  api_management_name        = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name        = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  display_name               = "B2B Api - ebIX"
  authorization_server_name  = data.azurerm_key_vault_secret.apim_oauth_server_name.value
  apim_logger_id             = data.azurerm_key_vault_secret.apim_logger_id.value
  logger_sampling_percentage = 100.0
  logger_verbosity           = "verbose"
  path                       = "ebix"
  api_type                   = "soap"
  backend_service_url        = "https://${module.func_receiver.default_hostname}"

  /* The WSDL schema import below works, but we're unsure if we gain any value from using it instead of creating the endpoints manually in terraform. Keeping it here for potential future use.
  import = {
    content_format = "wsdl"
    content_value  = <<-XML
      <?xml version="1.0" encoding="UTF-8"?>
      <wsdl:definitions xmlns:tns="urn:www:datahub:dk:b2b:service:v01" xmlns:wsdl="http://schemas.xmlsoap.org/wsdl/" xmlns:ns0="urn:www:datahub:dk:b2b:v01" xmlns:soap="http://schemas.xmlsoap.org/wsdl/soap/" name="Untitled" targetNamespace="urn:www:datahub:dk:b2b:service:v01">
        <wsdl:types>
          <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:b2b="urn:www:datahub:dk:b2b:v01" targetNamespace="urn:www:datahub:dk:b2b:v01" elementFormDefault="qualified" attributeFormDefault="unqualified">
            <xs:complexType name="DequeueMessageRequest_Type">
              <xs:sequence>
                <xs:element name="MessageId" type="b2b:MessageId_Type"/>
              </xs:sequence>
            </xs:complexType>
            <xs:complexType name="DequeueMessageResponse_Type"/>
            <xs:complexType name="GetMessageIdsRequest_Type">
              <xs:sequence>
                <xs:element name="From" type="b2b:DateTime_Type"/>
                <xs:element name="To" type="b2b:DateTime_Type"/>
              </xs:sequence>
            </xs:complexType>
            <xs:complexType name="GetMessageIdsResponse_Type">
              <xs:sequence>
                <xs:element name="MessageIdList" type="b2b:MessageIdList_Type"/>
              </xs:sequence>
            </xs:complexType>
            <xs:complexType name="GetMessageRequest_Type">
              <xs:sequence>
                <xs:element name="MessageId" type="b2b:MessageId_Type"/>
              </xs:sequence>
            </xs:complexType>
            <xs:complexType name="GetMessageResponse_Type">
              <xs:sequence>
                <xs:element name="MessageContainer" type="b2b:MessageContainer_Type"/>
              </xs:sequence>
            </xs:complexType>
            <xs:complexType name="MessageContainer_Type">
              <xs:sequence>
                <xs:element name="MessageReference" type="b2b:MessageReference_Type"/>
                <xs:element name="DocumentType" type="b2b:DocumentType_Type"/>
                <xs:element name="MessageType" type="b2b:MessageType_Type"/>
                <xs:element name="Payload" type="b2b:Payload_Type"/>
              </xs:sequence>
            </xs:complexType>
            <xs:complexType name="MessageIdList_Type">
              <xs:sequence>
                <xs:element name="MessageId" type="b2b:MessageId_Type" minOccurs="0" maxOccurs="unbounded"/>
              </xs:sequence>
            </xs:complexType>
            <xs:complexType name="Payload_Type">
              <xs:sequence>
                <xs:any processContents="skip" namespace="##any"/>
              </xs:sequence>
            </xs:complexType>
            <xs:complexType name="PeekMessageRequest_Type"/>
            <xs:complexType name="PeekMessageResponse_Type">
              <xs:sequence>
                <xs:element name="MessageContainer" type="b2b:MessageContainer_Type" minOccurs="0"/>
              </xs:sequence>
            </xs:complexType>
            <xs:complexType name="QueryDataRequest_Type">
              <xs:sequence>
                <xs:any minOccurs="0" maxOccurs="unbounded" processContents="skip" namespace="##any"/>
              </xs:sequence>
            </xs:complexType>
            <xs:complexType name="QueryDataResponse_Type">
              <xs:sequence>
                <xs:any minOccurs="0" maxOccurs="unbounded" processContents="skip" namespace="##any"/>
              </xs:sequence>
            </xs:complexType>
            <xs:complexType name="SendMessageRequest_Type">
              <xs:sequence>
                <xs:element name="MessageContainer" type="b2b:MessageContainer_Type"/>
              </xs:sequence>
            </xs:complexType>
            <xs:complexType name="SendMessageResponse_Type">
              <xs:sequence>
                <xs:element name="MessageId" type="b2b:MessageId_Type"/>
              </xs:sequence>
            </xs:complexType>
            <xs:element name="DequeueMessageRequest" type="b2b:DequeueMessageRequest_Type"/>
            <xs:element name="DequeueMessageResponse" type="b2b:DequeueMessageResponse_Type"/>
            <xs:element name="GetMessageIdsRequest" type="b2b:GetMessageIdsRequest_Type"/>
            <xs:element name="GetMessageIdsResponse" type="b2b:GetMessageIdsResponse_Type"/>
            <xs:element name="GetMessageRequest" type="b2b:GetMessageRequest_Type"/>
            <xs:element name="GetMessageResponse" type="b2b:GetMessageResponse_Type"/>
            <xs:element name="PeekMessageRequest" type="b2b:PeekMessageRequest_Type"/>
            <xs:element name="PeekMessageResponse" type="b2b:PeekMessageResponse_Type"/>
            <xs:element name="QueryDataRequest" type="b2b:QueryDataRequest_Type"/>
            <xs:element name="QueryDataResponse" type="b2b:QueryDataResponse_Type"/>
            <xs:element name="SendMessageRequest" type="b2b:SendMessageRequest_Type"/>
            <xs:element name="SendMessageResponse" type="b2b:SendMessageResponse_Type"/>
            <xs:element name="CData" type="b2b:CData_Type"/>
            <xs:simpleType name="DateTime_Type">
              <xs:restriction base="xs:dateTime"/>
            </xs:simpleType>
            <xs:simpleType name="DocumentType_Type">
              <xs:restriction base="xs:string">
                <xs:maxLength value="200"/>
              </xs:restriction>
            </xs:simpleType>
            <xs:simpleType name="MessageId_Type">
              <xs:restriction base="xs:string">
                <xs:maxLength value="35"/>
              </xs:restriction>
            </xs:simpleType>
            <xs:simpleType name="MessageReference_Type">
              <xs:restriction base="xs:string">
                <xs:maxLength value="35"/>
              </xs:restriction>
            </xs:simpleType>
            <xs:simpleType name="MessageType_Type">
              <xs:restriction base="xs:string">
                <xs:enumeration value="XML"/>
                <xs:enumeration value="EDIFACT"/>
              </xs:restriction>
            </xs:simpleType>
            <xs:simpleType name="CData_Type">
              <xs:restriction base="xs:string"/>
            </xs:simpleType>
          </xs:schema>
        </wsdl:types>
        <wsdl:message name="SendMessageRequest">
          <wsdl:part name="parameters" element="ns0:SendMessageRequest"/>
        </wsdl:message>
        <wsdl:message name="SendMessageResponse">
          <wsdl:part name="parameters" element="ns0:SendMessageResponse"/>
        </wsdl:message>
        <wsdl:message name="GetMessageRequest">
          <wsdl:part name="parameters" element="ns0:GetMessageRequest"/>
        </wsdl:message>
        <wsdl:message name="GetMessageResponse">
          <wsdl:part name="parameters" element="ns0:GetMessageResponse"/>
        </wsdl:message>
        <wsdl:message name="GetMessageIdsRequest">
          <wsdl:part name="parameters" element="ns0:GetMessageIdsRequest"/>
        </wsdl:message>
        <wsdl:message name="GetMessageIdsResponse">
          <wsdl:part name="parameters" element="ns0:GetMessageIdsResponse"/>
        </wsdl:message>
        <wsdl:message name="QueryDataRequest">
          <wsdl:part name="parameters" element="ns0:QueryDataRequest"/>
        </wsdl:message>
        <wsdl:message name="QueryDataResponse">
          <wsdl:part name="parameters" element="ns0:QueryDataResponse"/>
        </wsdl:message>
        <wsdl:message name="PeekMessageRequest">
          <wsdl:part name="parameters" element="ns0:PeekMessageRequest"/>
        </wsdl:message>
        <wsdl:message name="PeekMessageResponse">
          <wsdl:part name="parameters" element="ns0:PeekMessageResponse"/>
        </wsdl:message>
        <wsdl:message name="DequeueMessageRequest">
          <wsdl:part name="parameters" element="ns0:DequeueMessageRequest"/>
        </wsdl:message>
        <wsdl:message name="DequeueMessageResponse">
          <wsdl:part name="parameters" element="ns0:DequeueMessageResponse"/>
        </wsdl:message>
        <wsdl:portType name="marketMessagingB2BServiceV01PortType">
          <wsdl:operation name="sendMessage">
            <wsdl:input message="tns:SendMessageRequest"/>
            <wsdl:output message="tns:SendMessageResponse"/>
          </wsdl:operation>
          <wsdl:operation name="getMessage">
            <wsdl:input message="tns:GetMessageRequest"/>
            <wsdl:output message="tns:GetMessageResponse"/>
          </wsdl:operation>
          <wsdl:operation name="getMessageIds">
            <wsdl:input message="tns:GetMessageIdsRequest"/>
            <wsdl:output message="tns:GetMessageIdsResponse"/>
          </wsdl:operation>
          <wsdl:operation name="queryData">
            <wsdl:input message="tns:QueryDataRequest"/>
            <wsdl:output message="tns:QueryDataResponse"/>
          </wsdl:operation>
          <wsdl:operation name="peekMessage">
            <wsdl:input message="tns:PeekMessageRequest"/>
            <wsdl:output message="tns:PeekMessageResponse"/>
          </wsdl:operation>
          <wsdl:operation name="dequeueMessage">
            <wsdl:input message="tns:DequeueMessageRequest"/>
            <wsdl:output message="tns:DequeueMessageResponse"/>
          </wsdl:operation>
        </wsdl:portType>
        <wsdl:binding name="marketMessagingB2BServiceV01HTTPEndpointBinding" type="tns:marketMessagingB2BServiceV01PortType">
          <soap:binding style="document" transport="http://schemas.xmlsoap.org/soap/http"/>
          <wsdl:operation name="sendMessage">
            <soap:operation soapAction="sendMessage" style="document"/>
            <wsdl:input>
              <soap:body parts="parameters" use="literal"/>
            </wsdl:input>
            <wsdl:output>
              <soap:body parts="parameters" use="literal"/>
            </wsdl:output>
          </wsdl:operation>
          <wsdl:operation name="getMessage">
            <soap:operation soapAction="getMessage" style="document"/>
            <wsdl:input>
              <soap:body parts="parameters" use="literal"/>
            </wsdl:input>
            <wsdl:output>
              <soap:body parts="parameters" use="literal"/>
            </wsdl:output>
          </wsdl:operation>
          <wsdl:operation name="getMessageIds">
            <soap:operation soapAction="getMessageIds" style="document"/>
            <wsdl:input>
              <soap:body parts="parameters" use="literal"/>
            </wsdl:input>
            <wsdl:output>
              <soap:body parts="parameters" use="literal"/>
            </wsdl:output>
          </wsdl:operation>
          <wsdl:operation name="queryData">
            <soap:operation soapAction="queryData" style="document"/>
            <wsdl:input>
              <soap:body parts="parameters" use="literal"/>
            </wsdl:input>
            <wsdl:output>
              <soap:body parts="parameters" use="literal"/>
            </wsdl:output>
          </wsdl:operation>
          <wsdl:operation name="peekMessage">
            <soap:operation soapAction="peekMessage" style="document"/>
            <wsdl:input>
              <soap:body parts="parameters" use="literal"/>
            </wsdl:input>
            <wsdl:output>
              <soap:body parts="parameters" use="literal"/>
            </wsdl:output>
          </wsdl:operation>
          <wsdl:operation name="dequeueMessage">
            <soap:operation soapAction="dequeueMessage" style="document"/>
            <wsdl:input>
              <soap:body parts="parameters" use="literal"/>
            </wsdl:input>
            <wsdl:output>
              <soap:body parts="parameters" use="literal"/>
            </wsdl:output>
          </wsdl:operation>
        </wsdl:binding>
        <wsdl:service name="marketMessagingB2BServiceV01">
          <wsdl:port name="marketMessagingB2BServiceV01HTTPEndpoint" binding="tns:marketMessagingB2BServiceV01HTTPEndpointBinding">
            <!-- <soap:address location="https://apim-shared-sharedres-u-001.azure-api.net/ebix"/> // is this one needed? -->
          </wsdl:port>
        </wsdl:service>
      </wsdl:definitions>
      XML
  }
  */

  policies = [
    {
      xml_content = <<-XML
        <policies>
          <inbound>
            <trace source="B2B Api - ebIX" severity="verbose">
                <message>@{
                    string authHeader = context.Request.Headers.GetValueOrDefault("Authorization", "");
                    string callerId = "(empty)";
                    if (authHeader?.Length > 0)
                    {
                        string[] authHeaderParts = authHeader.Split(' ');
                        if (authHeaderParts?.Length == 2 && authHeaderParts[0].Equals("Bearer", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Jwt jwt;
                            if (authHeaderParts[1].TryParseJwt(out jwt))
                            {
                                callerId = (jwt.Claims.GetValueOrDefault("azp", "(empty)"));
                            }
                        }
                    }
                    return $"Caller ID (claims.azp): {callerId}";
                }</message>
                <metadata name="CorrelationId" value="@($"{context.RequestId}")" />
            </trace>
            <validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="Unauthorized. Failed policy requirements, or token is invalid or missing.">
                <openid-config url="https://login.microsoftonline.com/${data.azurerm_key_vault_secret.apim_b2c_tenant_id.value}/v2.0/.well-known/openid-configuration" />
                <required-claims>
                    <claim name="aud" match="any">
                        <value>${data.azurerm_key_vault_secret.apim_b2b_app_id.value}</value>
                    </claim>
                </required-claims>
            </validate-jwt>
            <base />
            <choose>
                <when condition="@(context.Request.Method == "POST")">
                    <check-header name="Content-Type" failed-check-httpcode="415" failed-check-error-message="Content-Type must be either application/ebix, text/xml or application/xml" ignore-case="true">
                      <value>application/ebix</value>
                      <value>text/xml</value>
                      <value>application/xml</value>
                    </check-header>
                    <set-variable name="bodySize" value="@(context.Request.Headers["Content-Length"][0])" />
                    <choose>
                        <when condition="@(int.Parse(context.Variables.GetValueOrDefault<string>("bodySize"))<52428800)">
                            <!--let it pass through by doing nothing-->
                        </when>
                        <otherwise>
                            <return-response>
                                <set-status code="413" reason="Payload Too Large" />
                                <set-body>@{
                                        return "Maximum allowed size for the POST requests is 52428800 bytes (50 MB). This request has size of "+ context.Variables.GetValueOrDefault<string>("bodySize") +" bytes";
                                    }</set-body>
                            </return-response>
                        </otherwise>
                    </choose>
                </when>
            </choose>
            <set-header name="CorrelationId" exists-action="override">
                <value>@($"{context.RequestId}")</value>
            </set-header>
            <set-header name="RequestTime" exists-action="override">
                <value>@(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"))</value>
            </set-header>
            <set-header name="Content-Type" exists-action="override">
                <value>application/ebix</value>
            </set-header>
            <set-header name="apim-operation-id" exists-action="override">
                <value>@(context.Operation.Id)</value>
            </set-header>
             <set-header name="apim-operation-name" exists-action="override">
                <value>@(context.Operation.Name)</value>
            </set-header>
          </inbound>
          <backend>
              <base />
          </backend>
          <outbound>
            <base />
            <set-header name="CorrelationId" exists-action="override">
              <value>@($"{context.RequestId}")</value>
            </set-header>
            <set-header name="content-type" exists-action="override">
              <value>text/xml</value>
            </set-header>
            <set-header name="apim-operation-id" exists-action="override">
              <value>@(context.Operation.Id)</value>
            </set-header>
            <set-header name="apim-operation-name" exists-action="override">
              <value>@(context.Operation.Name)</value>
            </set-header>
            <set-header name="Content-Type" exists-action="override">
              <value>text/xml</value>
            </set-header>
          </outbound>
          <on-error>
            <base />
            <set-header name="CorrelationId" exists-action="override">
                <value>@($"{context.RequestId}")</value>
            </set-header>
            <set-header name="apim-operation-id" exists-action="override">
              <value>@(context.Operation.Id)</value>
            </set-header>
            <set-header name="apim-operation-name" exists-action="override">
              <value>@(context.Operation.Name)</value>
            </set-header>
            <set-header name="Content-Type" exists-action="override">
              <value>text/xml</value>
            </set-header>
            <choose>
              <when condition="@(context.Response.StatusCode >= 500 && context.Response.StatusCode < 600)">
                <set-body template="liquid">
                  <soap-env:Envelope xmlns:soap-env="http://schemas.xmlsoap.org/soap/envelope/">
                    <soap-env:Body>
                      <soap-env:Fault>
                        <faultcode>soap-env:Client</faultcode>
                        <faultstring>B2B-900:{{context.LastError.Reason}}</faultstring>
                        <faultactor />
                      </soap-env:Fault>
                    </soap-env:Body>
                  </soap-env:Envelope>
                </set-body>
              </when>
            </choose>
          </on-error>
        </policies>
      XML
    }
  ]
}

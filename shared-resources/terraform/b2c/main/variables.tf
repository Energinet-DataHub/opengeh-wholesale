variable "b2c_tenant_id" {
  type        = string
  description = "Tenant ID of the B2C tenant instance."
}

variable "b2c_client_id" {
  type        = string
  description = "Client ID of the service principal managing resources in the B2C tenant."
}

variable "b2c_client_secret" {
  type        = string
  description = "The secret of the service principal managing resources in the B2C tenant."
  sensitive   = true
}

variable "b2c_tenant_name" {
  type        = string
  description = "The name of the B2C tenant, e.g. dev002DataHubB2C."
}

variable "mitid_client_id" {
  type        = string
  description = "Client id for OpenID Connect configuration of the MitID provider."
  sensitive   = true
}

variable "mitid_client_secret" {
  type        = string
  description = "Client secret for OpenID Connect configuration of the MitID provider."
  sensitive   = true
}

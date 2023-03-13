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
  description = "Client secret of the service principal managing resources in the B2C tenant"
}

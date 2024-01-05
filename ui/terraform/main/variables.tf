variable "subscription_id" {
  type        = string
  description = "Subscription that the infrastructure code is deployed into."
}

variable "resource_group_name" {
  type        = string
  description = "Resource Group that the infrastructure code is deployed into."
  default     = ""
}

variable "environment_short" {
  type        = string
  description = "Enviroment that the infrastructure code is deployed into."
}

variable "environment_instance" {
  type        = string
  description = "Enviroment instance that the infrastructure code is deployed into."
}

variable "domain_name_short" {
  type        = string
  description = "Shortest possible edition of the domain name."
}

variable "b2c_tenant_id" {
  type        = string
  description = "Tenant ID of the B2C tenant instance."
}

variable "b2c_client_id" {
  type        = string
  description = "Client ID of the service principal managing resources in the B2C tenant."
}

variable "b2c_tenant_name" {
  type        = string
  description = "The name of the B2C tenant, e.g. dev002DataHubB2C."
}

variable "azure_ad_security_group_id" {
  type        = string
  description = "The Id of the Azure Security group used for employees"
}

variable "enable_health_check_alerts" {
  type        = bool
  description = "Specify if health check alerts for Azure Functions and App Services should be enabled. Defaults to `true`"
  default     = true
}

variable "frontend_url" {
  description = "The URL of the frontend."
  type        = string
  default     = null
}

variable "ip_restrictions" {
  type = list(object({
    ip_address = string
    name       = string
    priority   = optional(number)
  }))
  description = "A list of IP restrictions defining allowed access to domain services. Each entry should include an 'ip_address' representing the allowed IP, a 'name' for identification, and an optional 'priority' for rule order. Defaults to `[]`."
  default     = []
}

variable "txt_value" {
  type        = string
  description = "The value of the TXT record used for production"
  default     = null
}

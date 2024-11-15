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

variable "location" {
  type        = string
  description = "The Azure region where the resources are created. Changing this forces a new resource to be created."
  default     = "West Europe"
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

variable "pim_reader_group_name" {
  type        = string
  description = "Name of the AD group with db_datareader permissions on the SQL database."
  default     = ""
}

variable "pim_contributor_data_plane_group_name" {
  type        = string
  description = "Name of the AD group with db_datawriter permissions on the SQL database."
  default     = ""
}

variable "pim_contributor_control_plane_group_name" {
  type        = string
  description = "Name of the PIM group that needs contributor control plane."
  default     = ""
}

variable "developer_security_group_name" {
  type        = string
  description = "Name of the Omada controlled security group containing developers to have access to the sub-system resources."
  default     = ""
}

variable "developer_security_group_contributor_access" {
  type        = bool
  description = "Flag to determine if the developers should have contributor access to the resource group."
  default     = false
}

variable "developer_security_group_reader_access" {
  type        = bool
  description = "Flag to determine if the developers should have reader access to the resource group."
  default     = false
}

variable "platform_security_group_name" {
  type        = string
  description = "Name of the Omada controlled security group containing platform developers to have access to the sub-system resources."
  default     = ""
}

variable "platform_security_group_contributor_access" {
  type        = bool
  description = "Flag to determine if the platform developers should have contributor access to the resource group."
  default     = false
}

variable "platform_security_group_reader_access" {
  type        = bool
  description = "Flag to determine if the platform developers should have reader access to the resource group."
  default     = false
}

variable "alert_email_address" {
  type        = string
  description = "(Optional) The email address to which alerts are sent."
  default     = null
}

variable "localhost_url" {
  type        = string
  description = "The URL of the localhost."
  default     = null
}

variable "budget_alert_amount" {
  type        = number
  description = "The budget amount for this subproduct"
}

variable "feature_management_use_process_manager" {
  type        = bool
  description = "When `true` then use Process Manager Client and API's instead of using Wholesale calculations API."
  default     = false
}

variable "subscription_id" {
  type        = string
  description = "Subscription that the infrastructure code is deployed into."
}

variable "resource_group_name" {
  type        = string
  description = "Resource Group that the infrastructure code is deployed into."
}

variable "environment_short" {
  type        = string
  description = "1 character name of the enviroment that the infrastructure code is deployed into."
}

variable "environment_instance" {
  type        = string
  description = "Enviroment instance that the infrastructure code is deployed into."
}

variable "domain_name_short" {
  type        = string
  description = "Shortest possible edition of the domain name."
}

variable "shared_resources_keyvault_name" {
  type        = string
  description = "Name of the KeyVault, that contains the shared secrets"
}

variable "shared_resources_resource_group_name" {
  type        = string
  description = "Name of the Resource Group, that contains the shared resources."
}

variable "b2c_tenant" {
  type        = string
  description = "URL of the Active Directory Tenant."
}

variable "b2c_spn_id" {
  type        = string
  description = "The Service Principal App Id of the Active Directory Tenant."
}

variable "b2c_spn_secret" {
  type        = string
  description = "The secret of the Service Principal of the Active Directory Tenant."
}

variable "enable_health_check_alerts" {
  type        = bool
  description = "Specify if health check alerts for Azure Functions and App Services should be enabled."
}

variable "developer_ad_group_name" {
  type        = string
  description = "(Optional) Name of the AD group containing developers to have read access to SQL database."
  default     = ""
}

variable "hosted_deployagent_public_ip_range" {
  type        = string
  description = "(Optional) Comma-delimited string with IPs / CIDR block with deployagent's public IPs, so it can access network-protected resources (Keyvaults, Function apps etc)"
  default     = null
}

variable "sendgrid_api_key" {
  type        = string
  description = "Sendgrid API Key"
}

variable "sendgrid_from_email" {
  type        = string
  description = "Specify the sender which the emails originates from"
}

variable "sendgrid_bcc_email" {
  type        = string
  description = "Specify the bcc email address for email copies"
}

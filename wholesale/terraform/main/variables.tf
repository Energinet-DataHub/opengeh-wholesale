variable "subscription_id" {
  type = string
}

variable "resource_group_name" {
  type = string
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
  description = "Name of the project this infrastructure is a part of."
}

variable "shared_resources_keyvault_name" {
  type        = string
  description = "Name of the Key Vault, that contains the shared secrets"
}

variable "shared_resources_resource_group_name" {
  type        = string
  description = "Name of the Resource Group, that contains the shared resources."
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

variable "tenant_id" {
  type        = string
  description = "Azure Tenant that the infrastructure is deployed into."
}

variable "developers_security_group_object_id" {
  type        = string
  description = "(Optional) The Object ID of the Azure AD security group containing DataHub developers."
  default     = null
}

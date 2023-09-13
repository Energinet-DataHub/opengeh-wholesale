variable "subscription_id" {
  type        = string
  description = "Subscription that the infrastructure code is deployed into."
}

variable "tenant_id" {
  type        = string
  description = "Azure Tenant that the infrastructure is deployed into."
}

variable "resource_group_name" {
  type        = string
  description = "Resource Group that the infrastructure code is deployed into."
  default = ""
}

variable "enable_health_check_alerts" {
  type        = bool
  description = "Specify if health check alerts for Azure Functions and App Services should be enabled."
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
  description = "Name of the Key Vault, that contains the shared secrets"
}

variable "shared_resources_resource_group_name" {
  type        = string
  description = "Name of the Resource Group, that contains the shared resources."
}

variable "alert_email_notification" {
  type        = string
  description = "Email address for the teams channel that the alerts are sent to."
}

variable "hosted_deployagent_public_ip_range" {
  type        = string
  description = "(Optional) Comma-delimited string with IPs / CIDR block with deployagent's public IPs, so it can access network-protected resources (Keyvaults, Function apps etc)"
  default     = null
}

variable "developers_security_group_object_id" {
  type        = string
  description = "(Optional) The Object ID of the Azure AD security group containing DataHub developers."
  default     = null
}

variable "migration_team_security_group_object_id" {
  type        = string
  description = "(Optional) The Object ID of the Azure AD security group containing Migrations team members."
  default     = null
}

variable "feature_flag_datahub2_healthcheck" {
  type        = bool
  description = "(Optional) Enables datahub2 healthcheck endpoint"
  default     = true
}

variable "datalake_readeraccess_group_name" {
  type        = string
  description = "(Required) The name of an AD group that should have Storage Blob Data Reader access to the Datalake storage account"
  default     = ""
}

variable "cert_pwd_migration_dh2_authentication_key1" {
  type        = string
  description = "Password for the certificate"
  default     = ""

  validation {
    condition     = length(var.cert_pwd_migration_dh2_authentication_key1) > 0
    error_message = "The password for the certificate must be specified."
  }
}

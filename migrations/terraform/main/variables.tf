variable "subscription_id" {
  type        = string
  description = "Subscription that the infrastructure code is deployed into."
}

variable "tenant_id" {
  type        = string
  description = "Azure Tenant that the infrastructure is deployed into."
}

variable "enable_health_check_alerts" {
  type        = bool
  description = "Specify if health check alerts for Azure Functions and App Services should be enabled."
  default     = true
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

variable "developers_security_group_object_id" {
  type        = string
  description = "(Optional) The Object ID of the Omada controlled security group containing DataHub developers."
  default     = null
}

variable "omada_developers_security_group_object_id" {
  type        = string
  description = "(Optional) The Object ID of the Azure AD security group containing DataHub developers."
  default     = null
}

variable "feature_flag_datahub2_time_series_import" {
  type        = bool
  description = "(Optional) Enables importing messages from DataHub 2 for time series synchronization"
  default     = false
}

variable "feature_flag_datahub2_healthcheck" {
  type        = bool
  description = "(Optional) Enables datahub2 healthcheck endpoint"
  default     = false
}

variable "cert_pwd_migration_dh2_authentication_key1" {
  type        = string
  description = "Password for the certificate"
  default     = ""
}

variable "databricks_vnet_address_space" {
  type        = string
  description = "Address space of the Virtual network where the Databricks Workspace is deployed."
}

variable "databricks_private_subnet_address_prefix" {
  type        = string
  description = "The address prefix of the private subnet used by Databricks."
}

variable "databricks_public_subnet_address_prefix" {
  type        = string
  description = "The address prefix of the public subnet used by Databricks."
}

variable "databricks_private_endpoints_subnet_address_prefix" {
  type        = string
  description = "The address prefix of the private endpoints subnet used by Databricks."
}

variable "github_username" {
  type        = string
  description = "Username used to access Github from Databricks jobs."
}

variable "github_personal_access_token" {
  type        = string
  description = "Personal access token for Github access"
}

variable "datahub2_ip_whitelist" {
  type        = string
  description = "Comma-delimited string with IPs / CIDR block with IPs that should be whitelisted for DataHub2"
  default     = null
}

variable "datahub2_migration_url" {
  type        = string
  description = "URL for DataHub2"
  default     = "https://example.com"
}

variable "developer_object_ids" {
  type        = list(string)
  description = "List of developer IDs to give access"
  default     = []
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

variable "databricks_group_id" {
  type        = string
  description = "The ID of the Databricks group containing Databricks users."
}

variable "databricks_group_id_migrations" {
  type        = string
  description = "The ID of the Databricks group containing Databricks users for migrations."
}

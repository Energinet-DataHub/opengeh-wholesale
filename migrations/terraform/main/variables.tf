variable "subscription_id" {
  type        = string
  description = "Subscription that the infrastructure code is deployed into."
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

variable "location" {
  type        = string
  description = "The Azure region where the resources are created. Changing this forces a new resource to be created."
  default     = "West Europe"
}

variable "domain_name_short" {
  type        = string
  description = "Shortest possible edition of the domain name."
}

variable "developer_security_group_name" {
  type        = string
  description = "Name of the Omada controlled security group containing developers to have access to the sub-system resources."
  default     = null
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

variable "feature_flag_datahub2_time_series_import" {
  type        = bool
  description = "(Optional) Enables importing messages from DataHub 2 for time series synchronization"
  default     = false
}

variable "feature_flag_purge_durable_function_history" {
  type        = bool
  description = "(Optional) Enables purge of durable function history for time series synchronization"
  default     = false
}

variable "feature_flag_datahub2_healthcheck" {
  type        = bool
  description = "(Optional) Enables datahub2 healthcheck endpoint"
  default     = false
}

variable "feature_flag_new_ts_orchestration" {
  type        = bool
  description = "(Optional) Enables the new time series orchestration"
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

variable "databricks_enable_verbose_audit_logs" {
  type        = bool
  description = "Flag to determine if verbose audit logs should be enabled for Databricks."
  default     = true
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

variable "alert_email_address" {
  type        = string
  description = "(Optional) The email address to which alerts are sent."
  default     = null
}

variable "function_app_sku_name" {
  type        = string
  description = "The SKU name of the function app."
  default     = "EP1"
}

variable "enable_audit_logs" {
  type        = bool
  description = "Should audit logs be enabled for the environment?"
}

variable "budget_alert_amount" {
  type        = number
  description = "The budget amount for this subproduct"
}

variable "activate_backup" {
  type        = bool
  description = "Flag to determine if back-ups of delta tables are enabled."
  default     = true
}

variable "databricks_readers_group" {
  type = object({
    id   = string
    name = string
  })
  description = "The Databricks group containing users with read permissions."
}

variable "databricks_contributor_dataplane_group" {
  type = object({
    id   = string
    name = string
  })
  description = "The Databricks group containing users with contributor permissions to the data plane."
}

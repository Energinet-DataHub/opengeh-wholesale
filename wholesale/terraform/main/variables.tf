variable "subscription_id" {
  type = string
}

variable "resource_group_name" { # Should be deleted when old subscriptions are deleted
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
  description = "Name of the project this infrastructure is a part of."
}

variable "location" {
  type        = string
  description = "The Azure region where the resources are created. Changing this forces a new resource to be created."
  default     = "West Europe"
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

variable "tenant_id" {
  type        = string
  description = "Azure Tenant that the infrastructure is deployed into."
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

variable "databricks_enable_verbose_audit_logs" {
  type        = bool
  description = "Flag to determine if verbose audit logs should be enabled for Databricks."
  default     = true
}

variable "calculation_input_folder" {
  type        = string
  description = "Name of input folder used by calculations running in Spark in Databricks."
  default     = "calculation_input"
}

variable "calculation_input_database" {
  type        = string
  description = "Name of database used by calculations running in Spark in Databricks."
  default     = "shared_wholesale_input"
}

variable "quarterly_resolution_transition_datetime" {
  type        = string
  description = "Start date 15 minuts imbalance settlement"
  default     = "2023-04-30T22:00:00Z"
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

variable "ip_restrictions" {
  type = list(object({
    ip_address = string
    name       = string
    priority   = optional(number)
  }))
  description = "A list of IP restrictions defining allowed access to domain services. Each entry should include an 'ip_address' representing the allowed IP, a 'name' for identification, and an optional 'priority' for rule order. Defaults to `[]`."
  default     = []
}

variable "alert_email_address" {
  type        = string
  description = "(Optional) The email address to which alerts are sent."
  default     = null
}

variable "datahub_bi_endpoint_enabled" {
  type        = bool
  description = "Flag to determine if the SQL warehouse for Datahub BI should be created"
  default     = false
}

variable "alert_email_address_edi" {
  type        = string
  description = "(Optional) The email address to which alerts are sent."
  default     = null
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

variable "settlement_report_auto_stop_minutes" {
  type        = number
  description = "Auto termination for Settlement Report SQL Warehouse, 0 means no auto termination."
  default     = 0
}

variable "setup_backup_sql_warehouse" {
  type        = bool
  description = "Flag to determine if a SQL warehouse for executing Databricks back ups should be created."
  default     = false
}

variable "enable_audit_logs" {
  type        = bool
  description = "Should audit logs be enabled for the environment?"
}

variable "budget_alert_amount" {
  type        = number
  description = "The budget amount for this subproduct"
}

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

variable "enable_health_check_alerts" {
  type        = bool
  description = "Specify if health check alerts for Azure Functions and App Services should be enabled. Defaults to `true`"
  default     = true
}

variable "developer_ad_group_name" {
  type        = string
  description = "(Optional) Name of the AD group containing developers to have read access to SQL database."
  default     = ""
}

variable "omada_developers_security_group_name" {
  type        = string
  description = "(Optional) Name of the Omada controlled security group containing developers to have access to the SQL database."
  default     = ""
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

variable "calculation_input_folder" {
  type        = string
  description = "Name of input folder used by calculations running in Spark in Databricks."
  default     = "calculation_input"
}

variable "quarterly_resolution_transition_datetime" {
  type        = string
  description = "Start date 15 minuts imbalance settlement"
  default = "2023-04-30T22:00:00Z"
}

variable "pim_sql_reader_ad_group_name" {
  type        = string
  description = "Name of the AD group with db_datareader permissions on the SQL database."
  default     = ""
}

variable "pim_sql_writer_ad_group_name" {
  type        = string
  description = "Name of the AD group with db_datawriter permissions on the SQL database."
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

variable "databricks_group_id" {
  type        = string
  description = "The ID of the Databricks group containing Databricks users."
}

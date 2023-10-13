variable "name" {
  type        = string
  description = "(Required) The name of the database."
}

variable "server_id" {
  type        = string
  description = "(Required) The Microsoft SQL Server ID"
}

variable "resource_group_name" {
  type        = string
  description = "(Required) The name of the resource group in which to create the database. This must be the same as Database Server resource group currently."
}

variable "location" {
  type        = string
  description = "(Required) Specifies the supported Azure location where the resource exists. Changing this forces a new resource to be created."
}

variable "server_name" {
  type        = string
  description = "(Required) The name of the SQL Server on which to create the database."
}

variable "dependencies" {
  type        = list(any)
  description = "A mapping of dependencies which this module depends on."
  default     = []
}

variable "max_size_gb" {
  type        = number
  description = "Max size of database"
  default     = 250
}

variable "sku_name" {
  type        = string
  description = "Database instance size"
  default     = "S0"
}

variable "long_term_retention_policy_monthly_retention" {
  type        = string
  description = "(Optional) Long therm retention policy monthly_retention"
  default     = null
}

variable "short_term_retention_policy_retention_days" {
  type        = number
  description = "(Optional) Short therm retention policy retention_days"
  default     = 7
}

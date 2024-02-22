variable "subscription_id" {
  type        = string
  description = "Subscription that the infrastructure code is deployed into."
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

variable "shared_resources_keyvault_name" {
  type        = string
  description = "Name of the KeyVault, that contains the shared secrets"
}

variable "shared_resources_resource_group_name" {
  type        = string
  description = "Name of the Resource Group, that contains the shared resources."
}

variable "ip_restrictions" {
  type        = list(object({
    ip_address  = string
    name        = string
    priority    = optional(number)
  }))
  description = "A list of IP restrictions defining allowed access to domain services. Each entry should include an 'ip_address' representing the allowed IP, a 'name' for identification, and an optional 'priority' for rule order. Defaults to `[]`."
  default     = []
}

variable "pim_sql_reader_ad_group_name" {
  type        = string
  description = "Name of the AD group with db_datareader permissions on the SQL database."
  default     = null
}

variable "pim_sql_writer_ad_group_name" {
  type        = string
  description = "Name of the AD group with db_datawriter permissions on the SQL database."
  default     = null
}

variable "developer_ad_group_name" {
  type        = string
  description = "(Optional) Name of the AD group containing developers to have read access to SQL database."
  default     = ""
}

variable "frontend_url" {
  description = "The URL of the frontend to Sauron."
  type        = string
  default     = null
}

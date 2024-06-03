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
  description = "1 character name of the enviroment that the infrastructure code is deployed into."
}

variable "environment_instance" {
  type        = string
  description = "Enviroment instance that the infrastructure code is deployed into."
}

variable "domain_name_short" {
  type        = string
  description = "Shortest possible edition of the domain name"
}

variable "omada_developers_security_group_name" {
  type        = string
  description = "(Optional) Name of the Omada controlled security group containing developers to have access to the SQL database."
  default     = ""
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

variable "feature_management_use_monthly_amount_per_charge_result_produced" {
  type        = bool
  description = "Should use monthly amount per charge result produced."
  default     = false
}

variable "feature_management_use_amount_per_charge_result_produced" {
  type        = bool
  description = "Should use amount per charge result produced."
  default     = false
}

variable "feature_management_use_request_wholesale_settlement_receiver" {
  type        = bool
  description = "Used to enable request wholesale settlement receiver."
  default     = false
}

variable "feature_management_use_message_delegation" {
  type        = bool
  description = "Used to enable message delegation for actors."
  default     = false
}

variable "feature_management_use_peek_messages" {
  type        = bool
  description = "Used to allow actors to peek messages."
  default     = false
}

variable "feature_management_use_request_messages" {
  type        = bool
  description = "Used to allow actors to request messages."
  default     = false
}

variable "feature_management_use_energy_result_produced" {
  type        = bool
  description = "Should use energy result produced."
  default     = false
}

variable "feature_management_use_total_monthly_amount_result_produced" {
  type        = bool
  description = "Should use total monthly amount result produced."
  default     = false
}

variable "feature_management_use_calculation_completed_event" {
  type        = bool
  description = "Should use Calculation Completed event sent from Wholesale."
  default     = false
}

variable "apim_maintenance_mode" {
  type        = bool
  description = "Determine if API Management is in maintenance mode. In maintenance mode all requests will return 503 Service Unavailable."
  default     = false
}

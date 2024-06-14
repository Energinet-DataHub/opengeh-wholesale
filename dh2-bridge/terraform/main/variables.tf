variable "subscription_id" {
  type        = string
  description = "Subscription that the infrastructure code is deployed into."
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

variable "region_short" {
  type        = string
  description = "Azure region that the infrastructure code is deployed into."
  default     = "we"
}

variable "omada_developers_security_group_name" {
  type        = string
  description = "(Optional) Name of the Omada controlled security group containing developers to have access to the SQL database."
  default     = ""
}

variable "sendgrid_api_key" {
  type        = string
  description = "Sendgrid API Key"
}

variable "sendgrid_to_email" {
  type        = string
  description = "Specify the to email address for email"
}

variable "sendgrid_from_email" {
  type        = string
  description = "Specify the sender which the emails originates from"
}

variable "pim_reader_group_name" {
  type        = string
  description = "Name of the AD group with db_datareader permissions on the SQL database."
  default     = ""
}

variable "pim_contributor_group_name" {
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

variable "dh2_bridge_recipient_party_gln" {
  type        = string
  description = "GLN of the recipient party."
  default     = null
}

variable "dh2_bridge_sender_party_gln" {
  type        = string
  description = "GLN of the sender party."
  default     = null
}

variable "dh2_endpoint" {
  type        = string
  description = "Endpoint for DH2"
  default     = null
}

variable "alert_email_address" {
  type        = string
  description = "(Optional) The email address to which alerts are sent."
  default     = null
}

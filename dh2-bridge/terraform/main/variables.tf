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

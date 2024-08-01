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

variable "biztalk_hybrid_connection_hostname" {
  type        = string
  description = "Hostname of the BizTalk hybrid connection."
}

variable "disable_biztalk_connection_check" {
  type        = string
  description = "Disable BizTalk connection health check."
  default     = false
}

variable "disable_biztalk_backoff" {
  type        = bool
  description = "Disable BizTalk Back Off."
  default     = false
}

variable "cert_esett_dh2_datahub3_password" {
  type        = string
  description = "Password for the eSett DH2 certificate"
}

variable "cert_esett_biztalk_datahub3_password" {
  type        = string
  description = "Password for the eSett Biztalk certificate"
}

variable "biz_talk_sender_code" {
  type        = string
  description = "Sender code for BizTalk"
  default     = null
}

variable "biz_talk_receiver_code" {
  type        = string
  description = "Receiver code for BizTalk"
  default     = null
}

variable "dh2_endpoint" {
  type        = string
  description = "Endpoint for DH2"
  default     = null
}

variable "biz_talk_biz_talk_end_point" {
  type        = string
  description = "Endpoint for BizTalk"
  default     = "/EL_DataHubService/IntegrationService.svc"
}

variable "biz_talk_business_type_consumption" {
  type        = string
  description = "Business type for consumption"
  default     = "NBS-RECI"
}

variable "biz_talk_business_type_production" {
  type        = string
  description = "Business type for production"
  default     = "NBS-MGXI"
}

variable "biz_talk_business_type_exchange" {
  type        = string
  description = "Business type for exchange"
  default     = "NBS-MEPI"
}

variable "alert_email_address" {
  type        = string
  description = "(Optional) The email address to which alerts are sent."
  default     = null
}

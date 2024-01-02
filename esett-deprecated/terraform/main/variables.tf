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

variable "cert_pwd_esett_dh2_authentication_key1" {
  type        = string
  description = "Password for the DH2certificate"
}

variable "cert_pwd_esett_biztalk_authentication_key1" {
  type        = string
  description = "Password for the Biztalk certificate"
}

variable "developer_object_ids" {
  type        = list(string)
  description = "List of developer principal IDs"
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

variable "ip_restrictions" {
  type        = list(object({
    ip_address  = string
    name        = string
    priority    = optional(number)
  }))
  description = "A list of IP restrictions defining allowed access to domain services. Each entry should include an 'ip_address' representing the allowed IP, a 'name' for identification, and an optional 'priority' for rule order. Defaults to `[]`."
  default     = []
}

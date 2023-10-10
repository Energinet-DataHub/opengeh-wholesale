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

variable "github_username" {
  type        = string
  description = "Username used to access Github from Databricks jobs."
}

variable "github_personal_access_token" {
  type        = string
  description = "Personal access token for Github access"
}

variable "developers_security_group_object_id" {
  type        = string
  description = "(Optional) The Object ID of the Azure AD security group containing DataHub developers."
  default     = null
}

variable "kv_secrets" {
  type = list(object({
    name  = string
    value = string
  }))
  description = "(Optional) App IDs and Client Secrets that can be used to retrieve access tokens for certain scenarios"
  default     = null
}

variable "b2c_kv_secrets" {
  type = list(object({
    name  = string
    value = string
  }))
  description = "(Optional) App IDs and Client Secrets that can be used to retrieve access tokens for certain scenarios"
  default     = null
}

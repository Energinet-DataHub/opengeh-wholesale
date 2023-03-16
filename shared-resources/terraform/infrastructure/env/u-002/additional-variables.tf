variable "tenant_id" {
  type        = string
  description = "Azure Tenant that the infrastructure is deployed into."
}

variable "github_username" {
  type        = string
  description = "Username used to access Github from Databricks jobs."
}

variable "github_personal_access_token" {
  type        = string
  description = "Personal access token for Github access"
}

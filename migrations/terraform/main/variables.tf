variable subscription_id {
  type        = string
  description = "Subscription that the infrastructure code is deployed into."
}

variable tenant_id {
  type        = string
  description = "Azure Tenant that the infrastructure is deployed into."
}

variable resource_group_name {
  type        = string
  description = "Resource Group that the infrastructure code is deployed into."
}

variable environment_short {
  type          = string
  description   = "1 character name of the enviroment that the infrastructure code is deployed into."
}

variable environment_instance {
  type          = string
  description   = "Enviroment instance that the infrastructure code is deployed into."
}

variable domain_name_short {
  type          = string
  description   = "Shortest possible edition of the domain name."
}

variable shared_resources_keyvault_name {
  type          = string
  description   = "Name of the Key Vault, that contains the shared secrets"
}

variable shared_resources_resource_group_name {
  type          = string
  description   = "Name of the Resource Group, that contains the shared resources."
}

variable github_username {
  type          = string
  description   = "Username used to access Github from Databricks jobs."
}

variable github_personal_access_token {
  type          = string
  description   = "Personal access token for Github access"
}
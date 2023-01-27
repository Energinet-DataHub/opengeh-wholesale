variable subscription_id {
  type        = string
  description = "Subscription that the infrastructure code is deployed into."
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
  description   = "Shortest possible edition of the domain name"
}

variable "shared_resources_keyvault_name" {
  type          = string
  description   = "Name of the Core Key Vault, that contains shared secrets"
}

variable shared_resources_resource_group_name {
  type          = string
  description   = "Resource group name of the Core Key Vaults location"
}

variable developer_ad_group_name {
  type          = string
  description   = "(Optional) Name of the AD group containing developers to have read access to SQL database."
  default       = ""
}

variable performance_test_enabled {
  type          = bool
  description   = "(Optional) Enables features needed for the Messaging.Api Performance Test"
  default       = false
}
variable "name" { # TODO: Delete when old subscriptions are removed
  type        = string
  description = "Specifies the name of the Key Vault. Changing this forces a new resource to be created."
  default     = ""
}

variable "project_name" {
  type        = string
  description = "Name of the project this infrastructure is a part of."
}

variable "environment_short" {
  type        = string
  description = "The short value name of the environment."
}

variable "environment_instance" {
  type        = string
  description = "The instance value of the environment."
}

variable "resource_group_name" {
  type        = string
  description = "The name of the resource group in which to create the Key Vault. Changing this forces a new resource to be created."
}

variable "location" {
  type        = string
  description = "The Azure region where the resources are created. Changing this forces a new resource to be created."
}

variable "sku_name" {
  type        = string
  description = "The Name of the SKU used for this Key Vault. Possible values are `standard` and `premium`."
}

variable "access_policies" {
  type = list(object({
    tenant_id               = string
    object_id               = string
    secret_permissions      = list(string)
    key_permissions         = list(string)
    certificate_permissions = list(string)
    storage_permissions     = list(string)
  }))
  description = "A list of objects describing the Key Vault access policies."
  default     = []
}

variable "enabled_for_template_deployment" {
  type        = bool
  description = "Boolean flag to specify whether Azure Resource Manager is permitted to retrieve secrets from the key vault. Defaults to false."
  default     = false
}

variable "enabled_for_deployment" {
  type        = bool
  description = "Boolean flag to specify whether Azure Resource Manager is permitted to retrieve certificate secrets from the key vault. Defaults to false."
  default     = false
}

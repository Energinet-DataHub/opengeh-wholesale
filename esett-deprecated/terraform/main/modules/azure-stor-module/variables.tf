variable "name" {}

variable "resource_group_name" {
  description = "name of the resource group"
}

variable "location" {}

variable "account_tier" {
  default = "Standard"
}

variable "account_replication_type" {
  default = "LRS"
}

variable "access_tier" {
  default = "Hot"
}

variable "account_kind" {
  default = "StorageV2"
}

variable "is_hns_enabled" {
  default = false
}

variable "https_only" {
  default = true
}

variable "assign_identity" {
  default = true
}

variable "blobs" {
  type    = list(any)
  default = []
}

variable "containers" {
  type = list(object({
    name        = string
    access_type = string
  }))
  default = []
}

variable "queues" {
  type    = list(string)
  default = []
}

variable "shares" {
  type = list(object({
    name  = string
    quota = number
  }))
  default = []
}

variable "tables" {
  type    = list(string)
  default = []
}

variable "dependencies" {
  type    = list(any)
  default = []
}

variable "blob_retention_days" {
  description = "Set block for delete_retention_policy if needed"
  default     = 0
}

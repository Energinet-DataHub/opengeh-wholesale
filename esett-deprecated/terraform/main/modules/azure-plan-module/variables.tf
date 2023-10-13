variable "name" {}

variable "resource_group_name" {
  description = "Name of the resource group"
}

variable "location" {}

variable "kind" {
  description = "The kind of the App Service Plan to create: FunctionApp, elastic, Linux, Windows."
  default     = "FunctionApp"
}

variable "maximum_elastic_worker_count" {
  type    = number
  default = 1
}

variable "tier" {
  default = "Free"
}

variable "size" {
  default = "B1"
}

variable "dependencies" {
  type    = list(any)
  default = []
}

variable "reserved" {
  type    = bool
  default = false
}

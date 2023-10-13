variable "dependencies" {
  type    = list(any)
  default = []
}

variable "name" {
  default = ""
}

variable "location" {
  default = "westeurope"
}

variable "resource_group_name" {}

variable "app_service_plan_id" {}

variable "app_version" {
  default = "~4"
}

variable "application_insights_instrumentation_key" {}

variable "app_settings" {
  type    = map(string)
  default = {}
}

variable "always_on" {
  default = true
}

variable "connection_strings" {
  default = {}
}

variable "os_type" {
  default = null
}

variable "pre_warmed_instance_count" {
  type    = number
  default = 1
}

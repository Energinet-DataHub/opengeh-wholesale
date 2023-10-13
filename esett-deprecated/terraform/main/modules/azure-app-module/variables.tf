variable "dependencies" {
  type    = list(any)
  default = []
}

variable "name" {}

variable "location" {}

variable "resource_group_name" {}

variable "app_service_plan_id" {}

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

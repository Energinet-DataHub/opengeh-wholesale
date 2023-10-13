variable "connection_string" {
  type        = string
  description = "(Required) The connection string for the storage account to which this SAS applies."
}

variable "container_name" {
  type        = string
  description = "(Required) Name of the container"
}

variable "ip_address" {
  type        = string
  description = "(Optional) Single ipv4 address or range (connected with a dash) of ipv4 addresses"
}

variable "start" {
  type        = string
  description = "(Required) The starting time and date of validity of this SAS"
}

variable "expiry" {
  type        = string
  description = "(Required) The expiration time and date of this SAS"
}

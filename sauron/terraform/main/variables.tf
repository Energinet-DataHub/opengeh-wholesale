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

variable "shared_resources_keyvault_name" {
  type        = string
  description = "Name of the KeyVault, that contains the shared secrets"
}

variable "shared_resources_resource_group_name" {
  type        = string
  description = "Name of the Resource Group, that contains the shared resources."
}

variable "hosted_deployagent_public_ip_range" {
  type        = string
  description = "(Optional) Comma-delimited string with IPs / CIDR block with deployagent's public IPs, so it can access network-protected resources (Keyvaults, Function apps etc)"
  default     = null
}

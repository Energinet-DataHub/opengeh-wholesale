variable "virtual_network_resource_group_name" {
  type        = string
  description = "Resource group name of the virtual network"
}

variable "virtual_network_name" {
  type        = string
  description = "Name of the virtual network"
}

variable "agents_address_space" {
  type        = string
  description = "Address space of the deployment agent subnet"
}

variable "vm_user_name" {
  type        = string
  description = "Deployment Agent VM user name for SSH connection."
}

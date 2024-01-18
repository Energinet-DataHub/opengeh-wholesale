variable "subscription_id" {
  type        = string
  description = "Subscription that the infrastructure code is deployed into."
}

variable "resource_group_name" { # Should be deleted when old subscriptions are deleted
  type        = string
  description = "Resource Group that the infrastructure code is deployed into."
  default     = ""
}

variable "environment_short" {
  type        = string
  description = "Enviroment that the infrastructure code is deployed into."
}

variable "environment_instance" {
  type        = string
  description = "Enviroment instance that the infrastructure code is deployed into."
}

variable "region_short" {
  type        = string
  description = "Azure region that the infrastructure code is deployed into."
  default     = "we"
}

variable "domain_name_short" {
  type        = string
  description = "Shortest possible edition of the domain name."
}

variable "project_name" {
  type = string
}

variable "arm_tenant_id" {
  type        = string
  description = "ID of the Azure tenant where the infrastructure is deployed"
}

variable "apim_publisher_email" {
  type        = string
  description = "(Required) The email of publisher/company."
}

variable "apim_maintenance_mode" {
  type        = bool
  description = "Determine if API Management is in maintenance mode. In maintenance mode all requests will return 503 Service Unavailable."
  default     = false
}

variable "apim_b2c_tenant_id" {
  type        = string
  description = "ID of the B2C tenant hosting the backend app registrations authorizing against."
}

variable "frontend_open_id_url" {
  type        = string
  description = "Open ID configuration URL used for authentication of the frontend."
}

variable "backend_b2b_app_id" {
  type        = string
  description = "The Application ID of the backend B2B app registration."
}

variable "backend_b2b_app_obj_id" {
  type        = string
  description = "The Object ID of the backend B2B app registration."
}

variable "backend_b2b_app_sp_id" {
  type        = string
  description = "The Object ID of the service principal for backend B2B app registration."
}

variable "backend_bff_app_id" {
  type        = string
  description = "The Application ID of the backend BFF app registration."
}

variable "backend_bff_app_sp_id" {
  type        = string
  description = "The Object ID of the service principal for backend BFF app registration."
}

variable "backend_bff_app_scope_id" {
  type        = string
  description = "The ID of the scope needed by the frontend app to access backend BFF app."
}

variable "backend_bff_app_scope" {
  type        = string
  description = "The qualified value of the scope needed by the frontend app to access backend BFF app."
}

variable "backend_timeseriesapi_app_id" {
  type        = string
  description = "The Application ID of the backend TimeSeriesApi app registration."
}

variable "eloverblik_timeseriesapi_client_app_id" {
  type        = string
  description = "The Application ID of the Eloverblik TimeSeriesApi client app registration."
}

variable "authentication_sign_in_user_flow_id" {
  type        = string
  description = "The id of the user flow used for signing users in."
}

variable "authentication_invitation_user_flow_id" {
  type        = string
  description = "The id of the user flow used for inviting users."
}

variable "authentication_mitid_invitation_user_flow_id" {
  type        = string
  description = "The id of the user flow used for inviting users for MitID."
}

variable "virtual_network_resource_group_name" {
  type        = string
  description = "Name of the resource group where the virtual network is deployed"
}

variable "virtual_network_name" {
  type        = string
  description = "Name of the virtual network"
}

variable "apim_address_space" {
  type        = string
  description = "Address space of the APIM subnet"
}

variable "private_endpoint_address_space" {
  type        = string
  description = "Address space of the private endpoint subnet"
}

variable "vnet_integration_address_space" {
  type        = string
  description = "Address space of the vnet integration subnet"
}

variable "log_retention_in_days" {
  type        = number
  description = "Number of days logs are retained in log analytics workspace"
  default     = 30
}

variable "ag_primary_email_address" {
  type        = string
  description = "Email address of primary action group to which alerts will be routed."
}

variable "developers_security_group_object_id" {
  type        = string
  description = "(Optional) The Object ID of the Azure AD security group containing DataHub developers."
  default     = null
}

variable "platform_team_security_group_object_id" {
  type        = string
  description = "(Optional) The Object ID of the Azure AD security group containing Outlaws developers."
  default     = null
}

variable "ad_group_directory_reader" {
  type        = string
  description = "(Optional) Name of a Active Directory group with the Directory Reader permission."
  default     = ""
}

variable "cert_b2b_datahub3_password" {
  type        = string
  description = "Password for the B2C Datahub 3 certificate."
  default     = null
}

variable "cert_ebix_datahub3_password" {
  type        = string
  description = "Password for the eBix Datahub 3 certificate."
  default     = null
}

variable "ip_restrictions" {
  type        = list(object({
    ip_address  = string
    name        = string
    priority    = optional(number)
  }))
  description = "A list of IP restrictions defining allowed access to domain services. Each entry should include an 'ip_address' representing the allowed IP, a 'name' for identification, and an optional 'priority' for rule order. Defaults to `[]`."
  default     = []
}

variable "biztalk_hybrid_connection_hostname" {
  type        = string
  description = "Hostname of the biztalk hybrid connection"
}

variable "domain_verification_code" {
  type        = string
  description = "Domain verification code for the domain name"
  default     = null
}

variable "cert_esett_biztalk_datahub3_password" {
  type        = string
  description = "Password for the eSett Biztalk certificate"
}

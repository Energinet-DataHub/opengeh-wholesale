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
  description   = "Enviroment that the infrastructure code is deployed into."
}

variable environment_instance {
  type          = string
  description   = "Enviroment instance that the infrastructure code is deployed into."
}

variable domain_name_short {
  type          = string
  description   = "Shortest possible edition of the domain name."
}

variable shared_resources_keyvault_name {
  type          = string
  description   = "Name of the KeyVault, that contains the shared secrets"
}

variable shared_resources_resource_group_name {
  type          = string
  description   = "Name of the Resource Group, that contains the shared resources."
}

variable apim_b2c_tenant_frontend_userflow {
  type          = string
  description   = "The URL of the B2C tenant where the signin user flow that is used by the frontend exists. On the format: https://<tenant-name>.b2clogin.com/<tenant-name>.onmicrosoft.com/<user-flow-name>"
}

variable azure_ad_security_group_id {
  type          = string
  description   = "The Id of the Azure Security group used for employees"
}

variable enable_health_check_alerts {
  type          = bool
  description   = "Specify if health check alerts for Azure Functions and App Services should be enabled."
}
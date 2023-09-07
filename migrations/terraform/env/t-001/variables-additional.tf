variable "datalake_readeraccess_group_name" {
  type        = string
  description = "(Required) The name of an AD group that should have Storage Blob Data Reader access to the Datalake storage account"
}

variable "cert_pwd_migration_dh2_authentication_key1" {
  type        = string
  description = "Password for the certificate"

  validation {
    condition     = length(var.cert_pwd_migration_dh2_authentication_key1) > 0
    error_message = "The password for the certificate must be specified."
  }
}

variable "cert_pwd_esett_dh2_authentication_key1" {
  type        = string
  description = "Password for the DataHub 2 certificate."

  validation {
    condition = length(var.cert_pwd_esett_dh2_authentication_key1) > 0
    error_message = "The password for the certificate must be specified."
  }
}

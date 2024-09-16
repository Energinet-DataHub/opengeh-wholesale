variable "cert_pwd_dh2bridge_dh2_authentication_key1" {
  type        = string
  description = "Password for the certificate"

  validation {
    condition = length(var.cert_pwd_dh2bridge_dh2_authentication_key1) > 0
    error_message = "The password for the certificate must be specified."
  }
}

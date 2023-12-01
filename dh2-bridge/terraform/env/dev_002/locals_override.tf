locals {
  DH2_ENDPOINT                      = "https://b2b.te6.datahub.dk"
  DH2BRIDGE_CERTIFICATE_THUMBPRINT  = resource.azurerm_key_vault_certificate.dh2_certificate.thumbprint
}

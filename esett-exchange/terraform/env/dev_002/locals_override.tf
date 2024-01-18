locals {
  DH2_ENDPOINT                    = "https://b2b.te6.datahub.dk"
  DH2_CERTIFICATE_THUMBPRINT      = resource.azurerm_key_vault_certificate.dh2_certificate.thumbprint
  BIZ_TALK_SENDER_CODE            = "45V0000000000601"
  BIZ_TALK_RECEIVER_CODE          = "44V000000000028C"
}

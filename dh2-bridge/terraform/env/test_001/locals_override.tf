locals {
  DH2_ENDPOINT                      = "https://b2b.te7.datahub.dk"
  DH2BRIDGE_CERTIFICATE_THUMBPRINT  = resource.azurerm_key_vault_certificate.dh2_certificate.thumbprint
  DH2_BRIDGE_RECIPIENT_PARTY_GLN    = "5790001330552"
  DH2_BRIDGE_SENDER_PARTY_GLN       = "45V000000000184K"
}

locals {
  DH2_ENDPOINT                 = "https://b2b.te6.datahub.dk"
  ESETT_CERTIFICATE_THUMBPRINT = resource.azurerm_key_vault_certificate.dh2_certificate.thumbprint
  ECP_BOOTSTRAP_SERVERS        = "kafka-online.streaming.dev.test.endk.local:9093"
  ECP_HEALTH_TOPIC             = "titans.connectivity.test"
}

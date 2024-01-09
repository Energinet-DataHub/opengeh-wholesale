locals {
  DH2_ENDPOINT                 = "https://b2b.te6.datahub.dk"
  ESETT_CERTIFICATE_THUMBPRINT = resource.azurerm_key_vault_certificate.dh2_certificate.thumbprint
  ECP_BOOTSTRAP_SERVERS        = "kafka-online.streaming.dev.test.endk.local:9093"
  ECP_HEALTH_TOPIC             = "titans.connectivity.test"
  BIZ_TALK_SENDER_CODE                  = "45V0000000000601"
  BIZ_TALK_RECEIVER_CODE                = "44V000000000028C"
  BIZ_TALK_BIZ_TALK_ROOT_URL            = "https://datahub.preproduction.biztalk.energinet.local"
}

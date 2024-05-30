locals {
  func_entrypoint_ecp_outbox = {
    app_settings = {
      WEBSITE_LOAD_CERTIFICATES = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=cert-esett-biztalk-thumbprint)"

      "DatabaseSettings:ConnectionString" = local.MS_ESETT_EXCHANGE_CONNECTION_STRING
      "BlobStorageSettings:AccountUri"    = local.ESETT_DOCUMENT_STORAGE_ACCOUNT_URI
      "BlobStorageSettings:ContainerName" = local.ESETT_DOCUMENT_STORAGE_CONTAINER_NAME

      "EcpSettings:SenderCode"                           = var.biz_talk_sender_code
      "EcpSettings:ReceiverCode"                         = var.biz_talk_receiver_code
      "EcpSettings:BiztalkRootUrl"                       = "https://${var.biztalk_hybrid_connection_hostname}"
      "EcpSettings:BizTalkEndPoint"                      = var.biz_talk_biz_talk_end_point
      "EcpSettings:BusinessTypeConsumption"              = var.biz_talk_business_type_consumption
      "EcpSettings:BusinessTypeProduction"               = var.biz_talk_business_type_production
      "EcpSettings:BusinessTypeExchange"                 = var.biz_talk_business_type_exchange
      "EcpSettings:DisableBizTalkBackOff"                = var.disable_biztalk_backoff
      "FeatureManagement__DisableBizTalkConnectionCheck" = var.disable_biztalk_connection_check
    }
  }
}

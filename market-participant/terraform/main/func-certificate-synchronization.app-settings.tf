locals {
  default_certificatesynchronization_app_settings = {
    "CertificateSynchronization:CertificatesKeyVault" = module.kv_dh2_certificates.vault_uri
    "CertificateSynchronization:ApimServiceName"      = data.azurerm_key_vault_secret.apim_instance_id.value
    "CertificateSynchronization:ApimTenantId"         = data.azurerm_subscription.this.tenant_id
    "CertificateSynchronization:ApimSpClientId"       = azuread_application.app_market_participant.client_id
    "CertificateSynchronization:ApimSpClientSecret"   = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_app_market_participant_password.name})"
  }
}

#
# Application registration with service principal
#
resource "azuread_application" "app_ci" {
  display_name = "sp-ci-${local.resource_suffix_with_dash}"
  owners = [
    data.azuread_client_config.this.object_id
  ]
}

resource "azuread_service_principal" "spn_ci" {
  application_id               = azuread_application.app_ci.application_id
  app_role_assignment_required = false
  owners = [
    data.azuread_client_config.this.object_id
  ]
}

resource "azuread_application_password" "ap_spn_ci" {
  application_object_id = azuread_application.app_ci.object_id
}

#
# Role assignments
#

resource "azurerm_role_assignment" "ra_ci" {
  scope                = azurerm_resource_group.this.id
  role_definition_name = "Contributor"
  principal_id         = azuread_service_principal.spn_ci.id
}

#
# Key Vault secrets
#

resource "azurerm_key_vault_secret" "kvs_shared_spn_id" {
  name         = "AZURE-SHARED-SPNID"
  value        = azuread_application.app_ci.application_id
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.kv_selfpermission
  ]
}

resource "azurerm_key_vault_secret" "kvs_shared_spn_secret" {
  name         = "AZURE-SHARED-SPNSECRET"
  value        = azuread_application_password.ap_spn_ci.value
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.kv_selfpermission
  ]
}

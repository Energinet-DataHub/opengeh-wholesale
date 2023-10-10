resource "azurerm_mssql_server" "this" {
  name                         = "mssql-${local.resource_suffix_with_dash}"
  location                     = azurerm_resource_group.this.location
  resource_group_name          = azurerm_resource_group.this.name
  version                      = "12.0"
  administrator_login          = local.integration_mssqlserver_admin_name
  administrator_login_password = random_password.integration_mssql_administrator_login_password.result

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }
}

resource "random_password" "integration_mssql_administrator_login_password" {
  length           = 16
  special          = true
  override_special = "_%@"
}

resource "azurerm_key_vault_secret" "kvs_mssql_admin_name" {
  name         = "mssql-admin-user-name"
  value        = local.integration_mssqlserver_admin_name
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

resource "azurerm_key_vault_secret" "kvs_mssql_admin_password" {
  name         = "mssql-admin-password"
  value        = random_password.integration_mssql_administrator_login_password.result
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

resource "azurerm_key_vault_secret" "kvs_mssql_server_id" {
  name         = "mssql-server-id"
  value        = azurerm_mssql_server.this.id
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

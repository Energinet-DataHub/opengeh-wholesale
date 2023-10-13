output "id" {
  value = azurerm_storage_account.main.id
}

output "name" {
  value = azurerm_storage_account.main.name
}

output "depended_on" {
  value = null_resource.dependency_setter.id
}

output "primary_connection_string" {
  value = azurerm_storage_account.main.primary_connection_string
}

output "primary_access_key" {
  value = azurerm_storage_account.main.primary_access_key
}

output "primary_blob_endpoint" {
  value = azurerm_storage_account.main.primary_blob_endpoint
}

output "containers" {
  value = {
    for c in azurerm_storage_container.main :
    c.name => {
      id   = c.id
      name = c.name
    }
  }
}

output "shares" {
  value = { for s in azurerm_storage_share.main :
    s.name => {
      id   = s.id
      name = s.name
    }
  }
}

output "tables" {
  value = { for t in azurerm_storage_table.main : t.name => t.id }
}

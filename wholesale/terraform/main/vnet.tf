# Create an A records pointing to the Blob private endpoints in the datalake in shared resources
resource "azurerm_private_dns_a_record" "st_data_lake_blob" {
  name                = data.azurerm_key_vault_secret.st_shared_data_lake_name.value
  zone_name           = "privatelink.blob.core.windows.net"
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  records = [
    data.azurerm_key_vault_secret.st_data_lake_blob_private_ip_address.value
  ]
}

# Create an A record pointing to the Data Lake File System Gen2 private endpoint
resource "azurerm_private_dns_a_record" "st_data_lake_dfs" {
  name                = data.azurerm_key_vault_secret.st_shared_data_lake_name.value
  zone_name           = "privatelink.dfs.core.windows.net"
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  records = [
    data.azurerm_key_vault_secret.st_data_lake_dfs_private_ip_address.value
  ]
}

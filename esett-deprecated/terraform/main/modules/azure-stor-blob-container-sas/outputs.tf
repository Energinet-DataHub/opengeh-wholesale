output "sas_url_query_string" {
  value = data.azurerm_storage_account_blob_container_sas.main.sas
}

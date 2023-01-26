resource "azurerm_role_assignment" "st_datalakemigrations_contributor" {
 scope                 = data.azurerm_key_vault_secret.st_data_lake_id.value 
 role_definition_name  = "Storage Blob Data Contributor"
 principal_id          = azuread_service_principal.spn_databricks.id
}
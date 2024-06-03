# The workspace is only created as a catalog must use an existing workspace, as it authenticates on a workspace level.
# It is only used for that and nothing else.

locals {
  credential_name = "dbw_${local.resources_suffix_no_dash}"
}

resource "azurerm_databricks_workspace" "this" {
  name                        = "dbw-${local.resources_suffix}"
  resource_group_name         = azurerm_resource_group.this.name
  location                    = azurerm_resource_group.this.location
  sku                         = "premium"
  managed_resource_group_name = "rg-dbw-${local.resources_suffix}"
}

# Wait for the workspace to create the access connector
resource "time_sleep" "wait_dbw_creation" {
  create_duration = "60s"

  depends_on = [azurerm_databricks_workspace.this]
}

# Access connector created by the workspace - used for accessing storage accounts
data "azurerm_databricks_access_connector" "this" {
  name                = "unity-catalog-access-connector" # Default created
  resource_group_name = "rg-dbw-${local.resources_suffix}"

  depends_on = [time_sleep.wait_dbw_creation]
}

# Give the access connector the necessary role on the shared datalake
resource "azurerm_role_assignment" "st_datalake_shres_contributor" {
  scope                = module.st_data_lake.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = data.azurerm_databricks_access_connector.this.identity[0].principal_id
}

# Wait for the role assignment to be created
resource "time_sleep" "wait_role_assignment" {
  create_duration = "60s"

  depends_on = [azurerm_role_assignment.st_datalake_shres_contributor]
}

# External location for the root of the shared catalog
resource "databricks_external_location" "datalake_shres" {
  provider        = databricks.dbw
  name            = "unityroot_${module.st_data_lake.name}"
  url             = "abfss://unityroot@${module.st_data_lake.name}.dfs.core.windows.net/"
  credential_name = local.credential_name
  comment         = "Managed by TF"
  depends_on      = [azurerm_databricks_workspace.this, time_sleep.wait_role_assignment]
}

# Shared catalog
resource "databricks_catalog" "shared" {
  provider       = databricks.dbw
  name           = "ctl_${local.resources_suffix_no_dash}"
  comment        = "Shared catalog in ${azurerm_databricks_workspace.this.name} workspace"
  owner          = data.azurerm_client_config.current.client_id
  storage_root   = databricks_external_location.datalake_shres.url
  isolation_mode = "ISOLATED"

  depends_on = [azurerm_databricks_workspace.this]
}

# Give SPN access to the catalog
resource "databricks_grant" "self" {
  provider   = databricks.dbw
  catalog    = databricks_catalog.shared.id
  principal  = data.azurerm_client_config.current.client_id
  privileges = ["ALL_PRIVILEGES"]

  depends_on = [azurerm_databricks_workspace.this]
}

# Give SPN access to the storage credential for future use
resource "databricks_grant" "self_storage_credential" {
  provider           = databricks.dbw
  storage_credential = local.credential_name
  principal          = data.azurerm_client_config.current.client_id
  privileges         = ["ALL_PRIVILEGES"]

  depends_on = [azurerm_databricks_workspace.this]
}

module "shared_unity_catalog_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=14.11.0"

  name         = "shared-unity-catalog-name"
  value        = databricks_catalog.shared.id # ID is the same as the name
  key_vault_id = module.kv_shared.id
}

module "unity_storage_credential_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=14.11.0"

  name         = "unity-storage-credential-id"
  value        = local.credential_name
  key_vault_id = module.kv_shared.id
}

module "shared_access_connector_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=14.11.0"

  name         = "shared-access-connector-principal-id"
  value        = data.azurerm_databricks_access_connector.this.identity[0].principal_id
  key_vault_id = module.kv_shared.id
}

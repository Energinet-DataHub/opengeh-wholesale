locals {
  # Locals determine which security groups that must be assigned grants on the shared Unity catalog.
  # This is necessary as reader and contributor groups may be the same on the development and test environments. In Databricks, the grants of a security group can't be be managed by multiple resources.
  # The legacy local (and the resources depending on it) will be removed when PIM on Databricks is rolled out.
  readers = var.databricks_readers_group.name == var.databricks_contributor_dataplane_group.name ? {} : { "${var.databricks_readers_group.name}" = "${var.databricks_readers_group.id}" }
  legacy  = "SEC-G-Datahub-DevelopersAzure" == var.databricks_contributor_dataplane_group.name ? {} : ("SEC-G-Datahub-DevelopersAzure" == var.databricks_readers_group.name ? {} : { "SEC-G-Datahub-DevelopersAzure" = "${var.databricks_developers_group_id}" })
}

# Configure SCIM synchronization or Databricks groups with reader and contributor permissions
resource "databricks_permission_assignment" "scim_reader" {
  for_each     = local.readers
  provider     = databricks.dbw
  principal_id = each.value
  permissions  = ["USER"]

  depends_on = [azurerm_databricks_workspace.this]
}

resource "databricks_permission_assignment" "scim_contributor_dataplane" {
  provider     = databricks.dbw
  principal_id = var.databricks_contributor_dataplane_group.id
  permissions  = ["USER"]

  depends_on = [azurerm_databricks_workspace.this]
}

# Grant access to the reader and contributor groups on the shared catalog
resource "databricks_grant" "reader_access_catalog" {
  for_each   = local.readers
  provider   = databricks.dbw
  catalog    = databricks_catalog.shared.id
  principal  = each.key
  privileges = ["USE_CATALOG", "SELECT", "READ_VOLUME", "USE_SCHEMA"]

  depends_on = [azurerm_databricks_workspace.this, databricks_permission_assignment.scim_reader, time_sleep.grant_sleep]
}

resource "databricks_grant" "contributor_dataplane_access_catalog" {
  provider   = databricks.dbw
  catalog    = databricks_catalog.shared.id
  principal  = var.databricks_contributor_dataplane_group.name
  privileges = ["USE_CATALOG", "SELECT", "READ_VOLUME", "USE_SCHEMA", "CREATE_TABLE", "MODIFY"]

  depends_on = [azurerm_databricks_workspace.this, databricks_permission_assignment.scim_contributor_dataplane, time_sleep.grant_sleep]
}

#
# Legacy. SCIM sync and grant access to the catalog for developers security group.
# TODO: remove when PIM on Databricks is rolled out
#
resource "null_resource" "scim_developers" {
  # Sync account level user into the workspace
  provisioner "local-exec" {
    interpreter = ["pwsh", "-Command"]
    command     = <<EOF
      # Get a token for the global Databricks application.
      # The resource name is fixed and never changes.
      $aadToken = $(az account get-access-token --resource=2ff814a6-3304-4ab8-85cb-cd0e6f879c1d --query accessToken --output tsv)

      $headers = @{
          'Authorization' = "Bearer $aadToken"
      }

      Invoke-RestMethod -Method PUT -Uri "https://${azurerm_databricks_workspace.this.workspace_url}/api/2.0/preview/permissionassignments/principals/${var.databricks_developers_group_id}" -Headers $headers -Body '{
        "permissions": [
            "USER"
        ]
      }'
    EOF
  }
}

# Grant developers access to the catalog
resource "databricks_grant" "developer_access_catalog" {
  for_each   = local.legacy
  provider   = databricks.dbw
  catalog    = databricks_catalog.shared.id
  principal  = each.key
  privileges = ["USE_CATALOG", "SELECT", "READ_VOLUME", "USE_SCHEMA"]

  depends_on = [azurerm_databricks_workspace.this, null_resource.scim_developers]
}

resource "time_sleep" "grant_sleep" {
  depends_on      = [databricks_grant.developer_access_catalog]
  create_duration = "60s"
}

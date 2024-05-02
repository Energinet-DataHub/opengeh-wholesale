# TODO: remove when we only have the OMADA group
resource "null_resource" "scim" {
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

      Invoke-RestMethod -Method PUT -Uri "https://${azurerm_databricks_workspace.this.workspace_url}/api/2.0/preview/permissionassignments/principals/${var.databricks_group_id}" -Headers $headers -Body '{
        "permissions": [
            "USER"
        ]
      }'
    EOF
  }
}

# TODO: remove when we only have the OMADA group
resource "databricks_grant" "dev_access_catalog" {
  provider   = databricks.dbw
  catalog    = databricks_catalog.shared.id
  principal  = "SEC-A-GreenForce-DevelopmentTeamAzure"
  privileges = ["USE_CATALOG", "SELECT", "READ_VOLUME", "USE_SCHEMA"]

  depends_on = [azurerm_databricks_workspace.this, null_resource.scim]
}

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
  provider   = databricks.dbw
  catalog    = databricks_catalog.shared.id
  principal  = "SEC-G-Datahub-DevelopersAzure"
  privileges = ["USE_CATALOG", "SELECT", "READ_VOLUME", "USE_SCHEMA"]

  depends_on = [azurerm_databricks_workspace.this, null_resource.scim_developers]
}

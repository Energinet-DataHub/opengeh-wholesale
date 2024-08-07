resource "null_resource" "cleanup_unity_catalog" {
  # External location and storage credential created by default is not deleted when the workspace is deleted, and the metastore has a cap of 200 in total. Therefore, we need to clean it up manually
  # https://docs.databricks.com/api/azure/workspace/storagecredentials/delete
  # https://docs.databricks.com/api/azure/workspace/externallocations/delete
  provisioner "local-exec" {
    command     = <<EOF
        try {
            Invoke-RestMethod -Method Delete -Uri "https://${azurerm_databricks_workspace.this.workspace_url}/api/2.1/unity-catalog/external-locations/${replace(azurerm_databricks_workspace.this.name, "-", "_")}?force=true" -Headers @{ Authorization = "Bearer ${databricks_token.pat.token_value}" }
            Invoke-RestMethod -Method Delete -Uri "https://${azurerm_databricks_workspace.this.workspace_url}/api/2.1/unity-catalog/storage-credentials/${replace(azurerm_databricks_workspace.this.name, "-", "_")}?force=true" -Headers @{ Authorization = "Bearer ${databricks_token.pat.token_value}" }
        } catch {
            Write-Host "Failed: cleanup unity catalog failed"
            exit 1
        }
      EOF
    interpreter = ["pwsh", "-Command"]
  }
  depends_on = [databricks_cluster.shared_all_purpose_integration_test, azurerm_databricks_workspace.this]
}

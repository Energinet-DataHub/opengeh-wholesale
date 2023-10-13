resource "null_resource" "dependency_getter" {
  provisioner "local-exec" {
    command = "echo ${length(var.dependencies)}"
  }
}

resource "null_resource" "dependency_setter" {
  depends_on = [
    azurerm_mssql_database.main,
  ]
}

resource "azurerm_mssql_database" "main" {
  depends_on  = [null_resource.dependency_getter]
  server_id   = var.server_id
  name        = var.name
  max_size_gb = var.max_size_gb
  sku_name    = var.sku_name

  long_term_retention_policy {
    week_of_year      = 1
    monthly_retention = var.long_term_retention_policy_monthly_retention
  }
  short_term_retention_policy {
    retention_days = var.short_term_retention_policy_retention_days
  }

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }
}

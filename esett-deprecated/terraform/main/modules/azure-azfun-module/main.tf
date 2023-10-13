resource "null_resource" "dependency_getter" {
  provisioner "local-exec" {
    command = "echo ${length(var.dependencies)}"
  }
}

resource "null_resource" "dependency_setter" {
  depends_on = [azurerm_function_app.main]
}

resource "azurerm_function_app" "main" {
  depends_on                 = [null_resource.dependency_getter]
  name                       = var.name
  location                   = var.location
  resource_group_name        = var.resource_group_name
  app_service_plan_id        = var.app_service_plan_id
  storage_account_name       = azurerm_storage_account.this.name
  storage_account_access_key = azurerm_storage_account.this.primary_access_key
  version                    = var.app_version
  https_only                 = true
  os_type                    = var.os_type
  app_settings = merge({
    APPINSIGHTS_INSTRUMENTATIONKEY = var.application_insights_instrumentation_key
  }, var.app_settings)

  dynamic "connection_string" {
    for_each = var.connection_strings
    content {
      name  = connection_string.key
      value = connection_string.value
      type  = "Custom"
    }
  }

  identity {
    type = "SystemAssigned"
  }

  site_config {
    always_on                 = var.always_on
    pre_warmed_instance_count = var.pre_warmed_instance_count
    scm_type                  = "VSTSRM"
    cors {
      allowed_origins = ["*"]
    }
  }

  lifecycle {
    ignore_changes = [
      tags,
      source_control
    ]
  }
}

resource "random_string" "this" {
  length  = 5
  special = false
  upper   = false
}

resource "azurerm_storage_account" "this" {
  name                     = "st${random_string.this.result}"
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
    ]
  }
}

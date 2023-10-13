resource "null_resource" "dependency_getter" {
  provisioner "local-exec" {
    command = "echo ${length(var.dependencies)}"
  }
}

resource "null_resource" "dependency_setter" {
  depends_on = [azurerm_app_service.main]
}

resource "azurerm_app_service" "main" {
  depends_on          = [null_resource.dependency_getter]
  name                = var.name
  location            = var.location
  resource_group_name = var.resource_group_name
  app_service_plan_id = var.app_service_plan_id

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
    dotnet_framework_version = "v4.0"
    scm_type                 = "LocalGit"
    always_on                = var.always_on
    min_tls_version          = "1.2"
    cors {
      allowed_origins = ["*"]
    }
  }

  lifecycle {
    ignore_changes = [
      tags,
      source_control,
      site_config[0].app_command_line, //Set by code deployment
      site_config[0].scm_type,         //Set by code deployment
    ]
  }
}

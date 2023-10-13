resource "null_resource" "dependency_getter" {
  provisioner "local-exec" {
    command = "echo ${length(var.dependencies)}"
  }
}

resource "null_resource" "dependency_setter" {
  depends_on = [azurerm_app_service_plan.main]
}

resource "azurerm_app_service_plan" "main" {
  depends_on                   = [null_resource.dependency_getter]
  name                         = var.name
  resource_group_name          = var.resource_group_name
  kind                         = var.kind
  location                     = var.location
  maximum_elastic_worker_count = var.maximum_elastic_worker_count
  reserved                     = var.reserved
  sku {
    tier = var.tier
    size = var.size
  }
  lifecycle {
    ignore_changes = [
      tags,
    ]
  }
}

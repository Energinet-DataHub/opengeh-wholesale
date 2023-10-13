resource "null_resource" "dependency_getter" {
  provisioner "local-exec" {
    command = "echo ${length(var.dependencies)}"
  }
}

resource "null_resource" "dependency_setter" {
  depends_on = [
    azurerm_storage_account.main,
    azurerm_storage_container.main,
    azurerm_storage_queue.main,
    azurerm_storage_share.main,
    azurerm_storage_table.main,
    azurerm_storage_blob.main
  ]
}

locals {
  blobs = [
    for b in var.blobs : merge({
      type         = "Block"
      size         = 0
      content_type = "application/octet-stream"
      source_file  = null
      source_uri   = null
      metadata     = {}
    }, b)
  ]
}

resource "azurerm_storage_account" "main" {
  name                      = var.name
  resource_group_name       = var.resource_group_name
  location                  = var.location
  account_kind              = var.account_kind
  account_tier              = var.account_tier
  account_replication_type  = var.account_replication_type
  access_tier               = var.access_tier
  enable_https_traffic_only = var.https_only
  is_hns_enabled            = var.is_hns_enabled
  min_tls_version           = "TLS1_2"
  identity {
    type = var.assign_identity ? "SystemAssigned" : null
  }
  dynamic "blob_properties" {
    for_each = var.blob_retention_days == 0 ? [] : [1]
    content {
      delete_retention_policy {
        days = var.blob_retention_days
      }
    }
  }
  depends_on = [null_resource.dependency_getter]

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }
}

resource "azurerm_storage_container" "main" {
  count                 = length(var.containers)
  name                  = var.containers[count.index].name
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = var.containers[count.index].access_type
  depends_on            = [azurerm_storage_account.main]
}

resource "azurerm_storage_queue" "main" {
  count                = length(var.queues)
  name                 = var.queues[count.index]
  storage_account_name = azurerm_storage_account.main.name
  depends_on           = [azurerm_storage_account.main]
}

resource "azurerm_storage_share" "main" {
  count                = length(var.shares)
  name                 = var.shares[count.index].name
  storage_account_name = azurerm_storage_account.main.name
  quota                = var.shares[count.index].quota
  depends_on           = [azurerm_storage_account.main]
}

resource "azurerm_storage_table" "main" {
  count                = length(var.tables)
  name                 = var.tables[count.index]
  storage_account_name = azurerm_storage_account.main.name
  depends_on           = [azurerm_storage_account.main]
}

resource "azurerm_storage_blob" "main" {
  count                  = length(local.blobs)
  name                   = local.blobs[count.index].name
  storage_account_name   = azurerm_storage_account.main.name
  storage_container_name = local.blobs[count.index].container_name
  type                   = local.blobs[count.index].type
  size                   = local.blobs[count.index].size
  content_type           = local.blobs[count.index].content_type
  source                 = local.blobs[count.index].source_file
  source_uri             = local.blobs[count.index].source_uri
  metadata               = local.blobs[count.index].metadata
  depends_on             = [azurerm_storage_account.main, azurerm_storage_container.main]
}

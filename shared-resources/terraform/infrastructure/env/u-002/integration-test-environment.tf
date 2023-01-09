# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
data "azurerm_client_config" "this" {}

resource "azurerm_resource_group" "integration-test-rg" {
  name      = "rg-DataHub-IntegrationTestResources-U-002"
  location  = "West Europe"
}

#
# Log Analytics Workspace and Application Insights
#
resource "azurerm_log_analytics_workspace" "integration-test-log" {
  name                = "log-integrationtest-u-002"
  location            = azurerm_resource_group.integration-test-rg.location
  resource_group_name = azurerm_resource_group.integration-test-rg.name
  sku                 = "PerGB2018"
  retention_in_days   = 30

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }
}

resource "azurerm_application_insights" "integration-test-appi" {
  name                = "appi-integrationtest-u-002"
  location            = azurerm_resource_group.integration-test-rg.location
  resource_group_name = azurerm_resource_group.integration-test-rg.name
  application_type    = "web"
  workspace_id        = azurerm_log_analytics_workspace.integration-test-log.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }
}

#
# Azure EventHub namespace
#
resource "azurerm_eventhub_namespace" "integration-test-evhns" {
  name                = "evhns-integrationstest-u-002"
  location            = azurerm_resource_group.integration-test-rg.location
  resource_group_name = azurerm_resource_group.integration-test-rg.name
  sku                 = "Standard"
  capacity            = 1

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }
}

#
# Azure ServiceBus namespace
#
resource "azurerm_servicebus_namespace" "integration-test-sbns" {
  name                = "sb-integrationtest-u-002"
  location            = azurerm_resource_group.integration-test-rg.location
  resource_group_name = azurerm_resource_group.integration-test-rg.name
  sku                 = "Premium"
  capacity            = 1

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }
}

#
# Key vault and access policies
#
resource "azurerm_key_vault" "integration-test-kv" {
  name                = "kv-integrationtest-u-002"
  location            = azurerm_resource_group.integration-test-rg.location
  resource_group_name = azurerm_resource_group.integration-test-rg.name
  tenant_id           = data.azurerm_client_config.this.tenant_id
  sku_name            = "standard"

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }
}

resource "azurerm_key_vault_access_policy" "integration-test-kv-selfpermissions" {
  key_vault_id            = azurerm_key_vault.integration-test-kv.id
  tenant_id               = data.azurerm_client_config.this.tenant_id
  object_id               = data.azurerm_client_config.this.object_id
  secret_permissions      = [
    "Delete",
    "List",
    "Get",
    "Set",
    "Purge",
  ]
}

resource "azurerm_key_vault_access_policy" "integration-test-kv-developer-ad-group" {
  key_vault_id            = azurerm_key_vault.integration-test-kv.id
  tenant_id               = data.azurerm_client_config.this.tenant_id
  object_id               = var.developers_security_group_object_id

  secret_permissions      = [
    "Get",
    "List",
  ]

  key_permissions         = [
    "Get",
    "List",
    "Update",
    "Create",
    "Delete",
    "Sign",
  ]
}

variable spn_ci_object_id {
  type          = string
  description   = "(Required) The Object ID of the Service principal running integration tests in CI pipelines."
}

resource "azurerm_key_vault_access_policy" "integration-test-kv-ci-test-spn" {
  key_vault_id            = azurerm_key_vault.integration-test-kv.id
  tenant_id               = data.azurerm_client_config.this.tenant_id
  object_id               = var.spn_ci_object_id

  secret_permissions      = [
    "Get",
    "List",
  ]

  key_permissions         = [
    "Get",
    "List",
    "Update",
    "Create",
    "Delete",
    "Sign",
  ]
}

resource "azurerm_role_assignment" "ci-spn-contributor-resource-group" {
  scope                = azurerm_resource_group.integration-test-rg.id
  role_definition_name = "Contributor"
  principal_id         = var.spn_ci_object_id
}

#
# Keyvault secrets
#
resource "azurerm_key_vault_secret" "kvs-appi-instrumentation-key" {
  name          = "AZURE-APPINSIGHTS-INSTRUMENTATIONKEY"
  value         = azurerm_application_insights.integration-test-appi.instrumentation_key
  key_vault_id  = azurerm_key_vault.integration-test-kv.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.integration-test-kv-selfpermissions
  ]
}

resource "azurerm_key_vault_secret" "kvs-evhns-connection-string" {
  name          = "AZURE-EVENTHUB-CONNECTIONSTRING"
  value         = azurerm_eventhub_namespace.integration-test-evhns.default_primary_connection_string
  key_vault_id  = azurerm_key_vault.integration-test-kv.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.integration-test-kv-selfpermissions
  ]
}

resource "azurerm_key_vault_secret" "kvs-log-workspace-id" {
  name          = "AZURE-LOGANALYTICS-WORKSPACE-ID"
  value         = azurerm_log_analytics_workspace.integration-test-log.workspace_id
  key_vault_id  = azurerm_key_vault.integration-test-kv.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.integration-test-kv-selfpermissions
  ]
}

resource "azurerm_key_vault_secret" "kvs-sbns-connection-string" {
  name          = "AZURE-SERVICEBUS-CONNECTIONSTRING"
  value         = azurerm_servicebus_namespace.integration-test-sbns.default_primary_connection_string
  key_vault_id  = azurerm_key_vault.integration-test-kv.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.integration-test-kv-selfpermissions
  ]
}

resource "azurerm_key_vault_secret" "kvs-resource-group-name" {
  name          = "AZURE-SHARED-RESOURCEGROUP"
  value         = azurerm_resource_group.integration-test-rg.name
  key_vault_id  = azurerm_key_vault.integration-test-kv.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.integration-test-kv-selfpermissions
  ]
}

resource "azurerm_key_vault_secret" "kvs-shared-spn-id" {
  name          = "AZURE-SHARED-SPNID"
  value         = data.azurerm_client_config.this.client_id
  key_vault_id  = azurerm_key_vault.integration-test-kv.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.integration-test-kv-selfpermissions
  ]
}

resource "azurerm_key_vault_secret" "kvs-shared-subscription-id" {
  name          = "AZURE-SHARED-SUBSCRIPTIONID"
  value         = data.azurerm_client_config.this.subscription_id
  key_vault_id  = azurerm_key_vault.integration-test-kv.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.integration-test-kv-selfpermissions
  ]
}

resource "azurerm_key_vault_secret" "kvs-shared-tenant-id" {
  name          = "AZURE-SHARED-TENANTID"
  value         = data.azurerm_client_config.this.tenant_id
  key_vault_id  = azurerm_key_vault.integration-test-kv.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.integration-test-kv-selfpermissions
  ]
}
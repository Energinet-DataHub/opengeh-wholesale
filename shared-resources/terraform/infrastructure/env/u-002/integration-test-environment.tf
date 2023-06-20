locals {
  integrationMsSqlServerAdminName = "inttestdbadmin"
}

data "azurerm_client_config" "this" {}

resource "azurerm_resource_group" "integration-test-rg" {
  name     = "rg-DataHub-IntegrationTestResources-U-002"
  location = "West Europe"
}

#
# Service principal running integration tests in CI pipelines.
#
variable "spn_ci_object_id" {
  type        = string
  description = "(Required) The Object ID of the Service principal running integration tests in CI pipelines."
}

resource "azurerm_role_assignment" "ci-spn-contributor-resource-group" {
  scope                = azurerm_resource_group.integration-test-rg.id
  role_definition_name = "Contributor"
  principal_id         = var.spn_ci_object_id
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
# SQL Server
#
resource "azurerm_mssql_server" "integration-sql-server" {
  name                         = "mssql-integrationtest-u-002"
  location                     = azurerm_resource_group.integration-test-rg.location
  resource_group_name          = azurerm_resource_group.integration-test-rg.name
  version                      = "12.0"
  administrator_login          = local.integrationMsSqlServerAdminName
  administrator_login_password = random_password.integration_mssql_administrator_login_password.result

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }
}

resource "random_password" "integration_mssql_administrator_login_password" {
  length           = 16
  special          = true
  override_special = "_%@"
}

#
# App service plan
#
resource "azurerm_service_plan" "integration-app-service-plan" {
  name                = "plan-integrationtest-u-002"
  location            = azurerm_resource_group.integration-test-rg.location
  resource_group_name = azurerm_resource_group.integration-test-rg.name
  os_type             = "Windows"
  sku_name            = "B1"

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
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
  key_vault_id = azurerm_key_vault.integration-test-kv.id
  tenant_id    = data.azurerm_client_config.this.tenant_id
  object_id    = data.azurerm_client_config.this.object_id
  secret_permissions = [
    "Delete",
    "List",
    "Get",
    "Set",
    "Purge",
  ]
}

resource "azurerm_key_vault_access_policy" "integration-test-kv-developer-ad-group" {
  key_vault_id = azurerm_key_vault.integration-test-kv.id
  tenant_id    = data.azurerm_client_config.this.tenant_id
  object_id    = var.developers_security_group_object_id

  secret_permissions = [
    "Get",
    "List",
  ]

  key_permissions = [
    "Get",
    "List",
    "Update",
    "Create",
    "Delete",
    "Sign",
  ]
}

resource "azurerm_key_vault_access_policy" "integration-test-kv-ci-test-spn" {
  key_vault_id = azurerm_key_vault.integration-test-kv.id
  tenant_id    = data.azurerm_client_config.this.tenant_id
  object_id    = var.spn_ci_object_id

  secret_permissions = [
    "Get",
    "List",
  ]

  key_permissions = [
    "Get",
    "List",
    "Update",
    "Create",
    "Delete",
    "Sign",
  ]
}

#
# Keyvault secrets
#
resource "azurerm_key_vault_secret" "kvs-appi-instrumentation-key" {
  name         = "AZURE-APPINSIGHTS-INSTRUMENTATIONKEY"
  value        = azurerm_application_insights.integration-test-appi.instrumentation_key
  key_vault_id = azurerm_key_vault.integration-test-kv.id

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
  name         = "AZURE-EVENTHUB-CONNECTIONSTRING"
  value        = azurerm_eventhub_namespace.integration-test-evhns.default_primary_connection_string
  key_vault_id = azurerm_key_vault.integration-test-kv.id

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
  name         = "AZURE-LOGANALYTICS-WORKSPACE-ID"
  value        = azurerm_log_analytics_workspace.integration-test-log.workspace_id
  key_vault_id = azurerm_key_vault.integration-test-kv.id

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
  name         = "AZURE-SERVICEBUS-CONNECTIONSTRING"
  value        = azurerm_servicebus_namespace.integration-test-sbns.default_primary_connection_string
  key_vault_id = azurerm_key_vault.integration-test-kv.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.integration-test-kv-selfpermissions
  ]
}

resource "azurerm_key_vault_secret" "kvs-sbns-namespace" {
  name         = "AZURE-SERVICEBUS-NAMESPACE"
  value        = azurerm_servicebus_namespace.integration-test-sbns.name
  key_vault_id = azurerm_key_vault.integration-test-kv.id

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
  name         = "AZURE-SHARED-RESOURCEGROUP"
  value        = azurerm_resource_group.integration-test-rg.name
  key_vault_id = azurerm_key_vault.integration-test-kv.id

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
  name         = "AZURE-SHARED-SPNID"
  value        = data.azurerm_client_config.this.client_id
  key_vault_id = azurerm_key_vault.integration-test-kv.id

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
  name         = "AZURE-SHARED-SUBSCRIPTIONID"
  value        = data.azurerm_client_config.this.subscription_id
  key_vault_id = azurerm_key_vault.integration-test-kv.id

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
  name         = "AZURE-SHARED-TENANTID"
  value        = data.azurerm_client_config.this.tenant_id
  key_vault_id = azurerm_key_vault.integration-test-kv.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.integration-test-kv-selfpermissions
  ]
}

resource "azurerm_key_vault_secret" "kvs-mssql-admin-name" {
  name         = "mssql-admin-user-name"
  value        = local.integrationMsSqlServerAdminName
  key_vault_id = azurerm_key_vault.integration-test-kv.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.integration-test-kv-selfpermissions
  ]
}

resource "azurerm_key_vault_secret" "kvs-mssql-admin-password" {
  name         = "mssql-admin-password"
  value        = random_password.integration_mssql_administrator_login_password.result
  key_vault_id = azurerm_key_vault.integration-test-kv.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.integration-test-kv-selfpermissions
  ]
}

resource "azurerm_key_vault_secret" "kvs-mssql-server-id" {
  name         = "mssql-server-id"
  value        = azurerm_mssql_server.integration-sql-server.id
  key_vault_id = azurerm_key_vault.integration-test-kv.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.integration-test-kv-selfpermissions
  ]
}

resource "azurerm_key_vault_secret" "kvs_dbw_sql_endpoint_id" {
  name         = "dbw-sql-endpoint-id"
  value        = databricks_sql_endpoint.sql_endpoint_integration_test.id
  key_vault_id = azurerm_key_vault.integration-test-kv.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.integration-test-kv-selfpermissions
  ]
}

resource "azurerm_key_vault_secret" "kvs_databricks_dbw_playground_workspace_token" {
  name         = "dbw-playground-workspace-token"
  value        = data.external.databricks_token_integration_test.result.pat_token
  key_vault_id = azurerm_key_vault.integration-test-kv.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.integration-test-kv-selfpermissions
  ]
}

resource "azurerm_key_vault_secret" "kvs_databricks_dbw_playground_workspace_url" {
  name         = "dbw-playground-workspace-url"
  value        = azurerm_databricks_workspace.integration-test-dbw.workspace_url
  key_vault_id = azurerm_key_vault.integration-test-kv.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.integration-test-kv-selfpermissions
  ]
}

resource "azurerm_key_vault_secret" "kvs_databricks_dbw_playground_workspace_id" {
  name         = "dbw-playground-workspace-id"
  value        = azurerm_databricks_workspace.integration-test-dbw.id
  key_vault_id = azurerm_key_vault.integration-test-kv.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.integration-test-kv-selfpermissions
  ]
}

resource "azurerm_key_vault_secret" "kvs_databricks_dbw_playground_storage_account_name" {
  name         = "dbw-playground-storage-account-name"
  value        = azurerm_storage_account.integration-test-st-databricks.name
  key_vault_id = azurerm_key_vault.integration-test-kv.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.integration-test-kv-selfpermissions
  ]
}

#
# Databricks related resources
#
resource "azurerm_storage_account" "integration-test-st-databricks" {
  name                     = "stdbwinttest"
  resource_group_name      = azurerm_resource_group.integration-test-rg.name
  location                 = azurerm_resource_group.integration-test-rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  account_kind             = "StorageV2"
  is_hns_enabled           = true

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }
}

data "azuread_client_config" "current" {}

resource "azuread_application" "app_databricks_migration" {
  display_name = "sp-databricks-migration-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  owners = [
    data.azuread_client_config.current.object_id
  ]
}

resource "azuread_service_principal" "spn_databricks_migration" {
  application_id               = azuread_application.app_databricks_migration.application_id
  app_role_assignment_required = false
  owners = [
    data.azuread_client_config.current.object_id
  ]
}

resource "azurerm_role_assignment" "ra_migrations_playground_contributor" {
  scope                = azurerm_storage_account.integration-test-st-databricks.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azuread_service_principal.spn_databricks_migration.id
}

resource "azurerm_storage_container" "playground" {
  name                  = "playground"
  storage_account_name  = azurerm_storage_account.integration-test-st-databricks.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "playground_timeseries_testdata" {
  name                  = "time-series-testdata"
  storage_account_name  = azurerm_storage_account.integration-test-st-databricks.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "playground_meteringpoints_testdata" {
  name                  = "metering-points-testdata"
  storage_account_name  = azurerm_storage_account.integration-test-st-databricks.name
  container_access_type = "private"
}

resource "azurerm_databricks_workspace" "integration-test-dbw" {
  name                = "dbw-integrationtest-u-002"
  resource_group_name = azurerm_resource_group.integration-test-rg.name
  location            = azurerm_resource_group.integration-test-rg.location
  sku                 = "premium"

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }
}

resource "azuread_application_password" "secret" {
  application_object_id = azuread_application.app_databricks_migration.object_id
}

#
# IMPORTANT:
# All 'databricks_' resources in the "Integration Test" environment
# MUST set provider alias; e.g. "provider = databricks.integration_test"
#
resource "databricks_sql_global_config" "sql_global_config_integration_test" {
  provider = databricks.integration_test

  security_policy = "DATA_ACCESS_CONTROL"
  data_access_config = {
    "spark.hadoop.fs.azure.account.auth.type.${azurerm_storage_account.integration-test-st-databricks.name}.dfs.core.windows.net" : "OAuth"
    "spark.hadoop.fs.azure.account.oauth.provider.type.${azurerm_storage_account.integration-test-st-databricks.name}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
    "spark.hadoop.fs.azure.account.oauth2.client.id.${azurerm_storage_account.integration-test-st-databricks.name}.dfs.core.windows.net" : databricks_secret.spn_app_id_integration_test.config_reference
    "spark.hadoop.fs.azure.account.oauth2.client.secret.${azurerm_storage_account.integration-test-st-databricks.name}.dfs.core.windows.net" : databricks_secret.spn_app_secret_integration_test.config_reference
    "spark.hadoop.fs.azure.account.oauth2.client.endpoint.${azurerm_storage_account.integration-test-st-databricks.name}.dfs.core.windows.net" : "https://login.microsoftonline.com/${data.azurerm_client_config.this.tenant_id}/oauth2/token"
  }

  enable_serverless_compute = true # Will be removed in v1.14.0 of the databricks provider
}

resource "databricks_git_credential" "ado_integration_test" {
  provider = databricks.integration_test

  git_username          = var.github_username
  git_provider          = "gitHub"
  personal_access_token = var.github_personal_access_token
}

resource "databricks_secret_scope" "migration_scope_integration_test" {
  provider = databricks.integration_test

  name = "migration-scope"
}

resource "databricks_secret" "spn_app_id_integration_test" {
  provider = databricks.integration_test

  key          = "spn_app_id"
  string_value = azuread_application.app_databricks_migration.application_id
  scope        = databricks_secret_scope.migration_scope_integration_test.id
}

resource "databricks_secret" "spn_app_secret_integration_test" {
  provider = databricks.integration_test

  key          = "spn_app_secret"
  string_value = azuread_application_password.secret.value
  scope        = databricks_secret_scope.migration_scope_integration_test.id
}

resource "databricks_secret" "appi_instrumentation_key_integration_test" {
  provider = databricks.integration_test

  key          = "appi_instrumentation_key"
  string_value = azurerm_application_insights.integration-test-appi.instrumentation_key
  scope        = databricks_secret_scope.migration_scope_integration_test.id
}

resource "databricks_secret" "st_dropzone_storage_account_integration_test" {
  provider = databricks.integration_test

  key          = "st_dropzone_storage_account"
  string_value = azurerm_storage_account.integration-test-st-databricks.name
  scope        = databricks_secret_scope.migration_scope_integration_test.id
}

resource "databricks_secret" "st_shared_datalake_account_integration_test" {
  provider = databricks.integration_test

  key          = "st_shared_datalake_account"
  string_value = azurerm_storage_account.integration-test-st-databricks.name
  scope        = databricks_secret_scope.migration_scope_integration_test.id
}

resource "databricks_secret" "st_migration_datalake_account_integration_test" {
  provider = databricks.integration_test

  key          = "st_migration_datalake_account"
  string_value = azurerm_storage_account.integration-test-st-databricks.name
  scope        = databricks_secret_scope.migration_scope_integration_test.id
}

resource "databricks_secret" "tenant_id_integration_test" {
  provider = databricks.integration_test

  key          = "tenant_id"
  string_value = data.azurerm_client_config.this.tenant_id
  scope        = databricks_secret_scope.migration_scope_integration_test.id
}

data "external" "databricks_token_integration_test" {
  program = ["pwsh", "${path.cwd}/scripts/generate-pat-token.ps1", azurerm_databricks_workspace.integration-test-dbw.id, "https://${azurerm_databricks_workspace.integration-test-dbw.workspace_url}"]
  depends_on = [
    azurerm_databricks_workspace.integration-test-dbw
  ]
}

data "databricks_spark_version" "latest_lts" {
  long_term_support = true
}

resource "databricks_instance_pool" "migration_pool_integration_test" {
  provider = databricks.integration_test

  instance_pool_name                    = "migration-playground-instance-pool"
  min_idle_instances                    = 0
  max_capacity                          = 5
  node_type_id                          = "Standard_DS3_v2"
  idle_instance_autotermination_minutes = 60
}

resource "databricks_job" "migration_workflow" {
  provider = databricks.integration_test

  name = "Landing_To_Wholesale_Gold_Fully_In_Playground"

  job_cluster {
    job_cluster_key = "playground_job_cluster"
    new_cluster {
      instance_pool_id = databricks_instance_pool.migration_pool_integration_test.id
      spark_version    = data.databricks_spark_version.latest_lts.id
      spark_conf = {
        "fs.azure.account.oauth2.client.endpoint.${azurerm_storage_account.integration-test-st-databricks.name}.dfs.core.windows.net" : "https://login.microsoftonline.com/${data.azurerm_client_config.this.tenant_id}/oauth2/token"
        "fs.azure.account.auth.type.${azurerm_storage_account.integration-test-st-databricks.name}.dfs.core.windows.net" : "OAuth"
        "fs.azure.account.oauth.provider.type.${azurerm_storage_account.integration-test-st-databricks.name}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
        "fs.azure.account.oauth2.client.id.${azurerm_storage_account.integration-test-st-databricks.name}.dfs.core.windows.net" : databricks_secret.spn_app_id_integration_test.config_reference
        "fs.azure.account.oauth2.client.secret.${azurerm_storage_account.integration-test-st-databricks.name}.dfs.core.windows.net" : databricks_secret.spn_app_secret_integration_test.config_reference
        "spark.databricks.delta.preview.enabled" : true
        "spark.databricks.io.cache.enabled" : true
        "spark.master" : "local[*, 4]"
      }
      spark_env_vars = {
        "APPI_INSTRUMENTATION_KEY"        = azurerm_application_insights.integration-test-appi.instrumentation_key
        "LANDING_STORAGE_ACCOUNT"         = azurerm_storage_account.integration-test-st-databricks.name
        "DATALAKE_STORAGE_ACCOUNT"        = azurerm_storage_account.integration-test-st-databricks.name
        "DATALAKE_SHARED_STORAGE_ACCOUNT" = azurerm_storage_account.integration-test-st-databricks.name
      }
    }
  }

  git_source {
    url      = "https://github.com/Energinet-DataHub/opengeh-migration.git"
    provider = "gitHub"
    branch   = "main"
  }

  task {
    task_key = "dummy_task_1"

    notebook_task {
      notebook_path = "dummy_task_1"
    }
    job_cluster_key = "playground_job_cluster"
  }

  depends_on = [
    databricks_instance_pool.migration_pool_integration_test
  ]
}

resource "databricks_sql_endpoint" "sql_endpoint_integration_test" {
  provider = databricks.integration_test

  name                      = "SQL Endpoint for Testing"
  cluster_size              = "Small"
  max_num_clusters          = 1
  auto_stop_mins            = 120
  enable_serverless_compute = true
  warehouse_type            = "PRO"

  # Enable preview as the statement API is currently in public preview
  channel {
    name = "CHANNEL_NAME_PREVIEW"
  }
}

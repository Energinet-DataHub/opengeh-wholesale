resource "azurerm_databricks_workspace" "this" {
  name                = "dbw-${local.resource_suffix_with_dash}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  sku                 = "premium"

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }
}

resource "databricks_sql_global_config" "sql_global_config_integration_test" {
  security_policy = "DATA_ACCESS_CONTROL"
  data_access_config = {
    "spark.hadoop.fs.azure.account.auth.type.${azurerm_storage_account.this.name}.dfs.core.windows.net" : "OAuth"
    "spark.hadoop.fs.azure.account.oauth.provider.type.${azurerm_storage_account.this.name}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
    "spark.hadoop.fs.azure.account.oauth2.client.id.${azurerm_storage_account.this.name}.dfs.core.windows.net" : databricks_secret.spn_app_id_integration_test.config_reference
    "spark.hadoop.fs.azure.account.oauth2.client.secret.${azurerm_storage_account.this.name}.dfs.core.windows.net" : databricks_secret.spn_app_secret_integration_test.config_reference
    "spark.hadoop.fs.azure.account.oauth2.client.endpoint.${azurerm_storage_account.this.name}.dfs.core.windows.net" : "https://login.microsoftonline.com/${data.azurerm_client_config.this.tenant_id}/oauth2/token"
  }

  enable_serverless_compute = true # Will be removed in v1.14.0 of the databricks provider
}

resource "databricks_git_credential" "ado_integration_test" {
  git_username          = var.github_username
  git_provider          = "gitHub"
  personal_access_token = var.github_personal_access_token
}

resource "databricks_secret_scope" "migration_scope_integration_test" {
  name = "migration-scope"
}

resource "databricks_secret" "spn_app_id_integration_test" {
  key          = "spn_app_id"
  string_value = azuread_application.app_ci.application_id
  scope        = databricks_secret_scope.migration_scope_integration_test.id
}

resource "databricks_secret" "spn_app_secret_integration_test" {
  key          = "spn_app_secret"
  string_value = azuread_application_password.ap_spn_ci.value
  scope        = databricks_secret_scope.migration_scope_integration_test.id
}

resource "databricks_secret" "appi_instrumentation_key_integration_test" {
  key          = "appi_instrumentation_key"
  string_value = azurerm_application_insights.this.instrumentation_key
  scope        = databricks_secret_scope.migration_scope_integration_test.id
}

resource "databricks_secret" "st_dh2data_storage_account_integration_test" {
  key          = "st_dh2data_storage_account"
  string_value = azurerm_storage_account.this.name
  scope        = databricks_secret_scope.migration_scope_integration_test.id
}

resource "databricks_secret" "st_shared_datalake_account_integration_test" {
  key          = "st_shared_datalake_account"
  string_value = azurerm_storage_account.this.name
  scope        = databricks_secret_scope.migration_scope_integration_test.id
}

resource "databricks_secret" "st_migration_datalake_account_integration_test" {
  key          = "st_migration_datalake_account"
  string_value = azurerm_storage_account.this.name
  scope        = databricks_secret_scope.migration_scope_integration_test.id
}

resource "databricks_secret" "tenant_id_integration_test" {
  key          = "tenant_id"
  string_value = data.azurerm_client_config.this.tenant_id
  scope        = databricks_secret_scope.migration_scope_integration_test.id
}

data "external" "databricks_token_integration_test" {
  program = ["pwsh", "${path.cwd}/scripts/generate-pat-token.ps1", azurerm_databricks_workspace.this.id, "https://${azurerm_databricks_workspace.this.workspace_url}"]
  depends_on = [
    azurerm_databricks_workspace.this
  ]
}

data "databricks_spark_version" "latest_lts" {
  long_term_support = true

  depends_on = [
    azurerm_databricks_workspace.this
  ]
}

resource "databricks_instance_pool" "migration_pool_integration_test" {
  instance_pool_name                    = "migration-playground-instance-pool"
  min_idle_instances                    = 0
  max_capacity                          = 5
  node_type_id                          = "Standard_DS3_v2"
  idle_instance_autotermination_minutes = 60
}

resource "databricks_job" "migration_workflow" {
  name = "Landing_To_Wholesale_Gold_Fully_In_Playground"

  job_cluster {
    job_cluster_key = "playground_job_cluster"
    new_cluster {
      instance_pool_id = databricks_instance_pool.migration_pool_integration_test.id
      spark_version    = data.databricks_spark_version.latest_lts.id
      spark_conf = {
        "fs.azure.account.oauth2.client.endpoint.${azurerm_storage_account.this.name}.dfs.core.windows.net" : "https://login.microsoftonline.com/${data.azurerm_client_config.this.tenant_id}/oauth2/token"
        "fs.azure.account.auth.type.${azurerm_storage_account.this.name}.dfs.core.windows.net" : "OAuth"
        "fs.azure.account.oauth.provider.type.${azurerm_storage_account.this.name}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
        "fs.azure.account.oauth2.client.id.${azurerm_storage_account.this.name}.dfs.core.windows.net" : databricks_secret.spn_app_id_integration_test.config_reference
        "fs.azure.account.oauth2.client.secret.${azurerm_storage_account.this.name}.dfs.core.windows.net" : databricks_secret.spn_app_secret_integration_test.config_reference
        "spark.databricks.delta.preview.enabled" : true
        "spark.databricks.io.cache.enabled" : true
        "spark.master" : "local[*, 4]"
      }
      spark_env_vars = {
        "APPI_INSTRUMENTATION_KEY"        = azurerm_application_insights.this.instrumentation_key
        "LANDING_STORAGE_ACCOUNT"         = azurerm_storage_account.this.name
        "DATALAKE_STORAGE_ACCOUNT"        = azurerm_storage_account.this.name
        "DATALAKE_SHARED_STORAGE_ACCOUNT" = azurerm_storage_account.this.name
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

resource "databricks_cluster" "shared_all_purpose_integration_test" {
  cluster_name     = "Shared all-purpose"
  num_workers      = 1
  instance_pool_id = databricks_instance_pool.migration_pool_integration_test.id
  spark_version    = data.databricks_spark_version.latest_lts.id
  spark_conf = {
    "fs.azure.account.oauth2.client.endpoint.${azurerm_storage_account.this.name}.dfs.core.windows.net" : "https://login.microsoftonline.com/${data.azurerm_client_config.this.tenant_id}/oauth2/token"
    "fs.azure.account.auth.type.${azurerm_storage_account.this.name}.dfs.core.windows.net" : "OAuth"
    "fs.azure.account.oauth.provider.type.${azurerm_storage_account.this.name}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
    "fs.azure.account.oauth2.client.id.${azurerm_storage_account.this.name}.dfs.core.windows.net" : databricks_secret.spn_app_id_integration_test.config_reference
    "fs.azure.account.oauth2.client.secret.${azurerm_storage_account.this.name}.dfs.core.windows.net" : databricks_secret.spn_app_secret_integration_test.config_reference
    "spark.databricks.delta.preview.enabled" : true
    "spark.databricks.io.cache.enabled" : true
    "spark.master" : "local[*, 4]"
  }
  spark_env_vars = {
    "APPI_INSTRUMENTATION_KEY"        = azurerm_application_insights.this.instrumentation_key
    "LANDING_STORAGE_ACCOUNT"         = azurerm_storage_account.this.name
    "DATALAKE_STORAGE_ACCOUNT"        = azurerm_storage_account.this.name
    "DATALAKE_SHARED_STORAGE_ACCOUNT" = azurerm_storage_account.this.name
  }
}

resource "azurerm_key_vault_secret" "kvs_dbw_sql_endpoint_id" {
  name         = "dbw-sql-endpoint-id"
  value        = databricks_sql_endpoint.sql_endpoint_integration_test.id
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.kv_selfpermission
  ]
}

resource "azurerm_key_vault_secret" "kvs_databricks_dbw_playground_workspace_token" {
  name         = "dbw-playground-workspace-token"
  value        = data.external.databricks_token_integration_test.result.pat_token
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.kv_selfpermission
  ]
}

resource "azurerm_key_vault_secret" "kvs_databricks_dbw_playground_workspace_url" {
  name         = "dbw-playground-workspace-url"
  value        = azurerm_databricks_workspace.this.workspace_url
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.kv_selfpermission
  ]
}

resource "azurerm_key_vault_secret" "kvs_databricks_dbw_playground_workspace_id" {
  name         = "dbw-playground-workspace-id"
  value        = azurerm_databricks_workspace.this.id
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.kv_selfpermission
  ]
}

resource "azurerm_key_vault_secret" "kvs_databricks_dbw_playground_storage_account_name" {
  name         = "dbw-playground-storage-account-name"
  value        = azurerm_storage_account.this.name
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.kv_selfpermission
  ]
}

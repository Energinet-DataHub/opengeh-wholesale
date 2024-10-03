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

  tags = local.tags
}

resource "databricks_permission_assignment" "developers" {
  principal_id = "729028915538231" # Databricks Group ID for the developer group
  permissions  = ["USER"]

  depends_on = [azurerm_databricks_workspace.this, databricks_token.pat]
}

resource "databricks_grant" "developer_access_catalog" {
  catalog    = local.databricks_unity_catalog_name
  principal  = "SEC-G-Datahub-DevelopersAzure"
  privileges = ["USE_CATALOG", "SELECT", "READ_VOLUME", "USE_SCHEMA"]

  depends_on = [azurerm_databricks_workspace.this, databricks_permission_assignment.developers, databricks_token.pat]
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

  depends_on = [azurerm_databricks_workspace.this, databricks_token.pat]
}

resource "databricks_git_credential" "ado_integration_test" {
  git_username          = var.github_username
  git_provider          = "gitHub"
  personal_access_token = var.github_personal_access_token

  depends_on = [azurerm_databricks_workspace.this, databricks_token.pat]
}

resource "databricks_secret_scope" "migration_scope_integration_test" {
  name = "migration-scope"

  depends_on = [azurerm_databricks_workspace.this, databricks_token.pat]
}

resource "databricks_secret" "spn_app_id_integration_test" {
  key          = "spn_app_id"
  string_value = azuread_application.app_ci.client_id
  scope        = databricks_secret_scope.migration_scope_integration_test.id

  depends_on = [azurerm_databricks_workspace.this, databricks_token.pat]
}

resource "databricks_secret" "spn_app_secret_integration_test" {
  key          = "spn_app_secret"
  string_value = azuread_application_password.ap_spn_ci.value
  scope        = databricks_secret_scope.migration_scope_integration_test.id

  depends_on = [azurerm_databricks_workspace.this, databricks_token.pat]
}

resource "databricks_secret" "appi_instrumentation_key_integration_test" {
  key          = "appi_instrumentation_key"
  string_value = azurerm_application_insights.this.instrumentation_key
  scope        = databricks_secret_scope.migration_scope_integration_test.id

  depends_on = [azurerm_databricks_workspace.this, databricks_token.pat]
}

resource "databricks_secret" "st_dh2data_storage_account_integration_test" {
  key          = "st_dh2data_storage_account"
  string_value = azurerm_storage_account.this.name
  scope        = databricks_secret_scope.migration_scope_integration_test.id

  depends_on = [azurerm_databricks_workspace.this, databricks_token.pat]
}

resource "databricks_secret" "st_shared_datalake_account_integration_test" {
  key          = "st_shared_datalake_account"
  string_value = azurerm_storage_account.this.name
  scope        = databricks_secret_scope.migration_scope_integration_test.id

  depends_on = [azurerm_databricks_workspace.this, databricks_token.pat]
}

resource "databricks_secret" "st_migration_datalake_account_integration_test" {
  key          = "st_migration_datalake_account"
  string_value = azurerm_storage_account.this.name
  scope        = databricks_secret_scope.migration_scope_integration_test.id

  depends_on = [azurerm_databricks_workspace.this, databricks_token.pat]
}

resource "databricks_secret" "tenant_id_integration_test" {
  key          = "tenant_id"
  string_value = data.azurerm_client_config.this.tenant_id
  scope        = databricks_secret_scope.migration_scope_integration_test.id

  depends_on = [azurerm_databricks_workspace.this, databricks_token.pat]
}

resource "databricks_secret" "resource_group_name_integration_test" {
  key          = "resource_group_name"
  string_value = azurerm_resource_group.this.name
  scope        = databricks_secret_scope.migration_scope_integration_test.id

  depends_on = [azurerm_databricks_workspace.this, databricks_token.pat]
}

resource "databricks_secret" "subscription_id_integration_test" {
  key          = "subscription_id"
  string_value = data.azurerm_client_config.this.subscription_id
  scope        = databricks_secret_scope.migration_scope_integration_test.id

  depends_on = [azurerm_databricks_workspace.this, databricks_token.pat]
}

resource "databricks_secret" "location_integration_test" {
  key          = "location"
  string_value = azurerm_resource_group.this.location
  scope        = databricks_secret_scope.migration_scope_integration_test.id

  depends_on = [azurerm_databricks_workspace.this, databricks_token.pat]
}

resource "time_rotating" "this" {
  rotation_days = 45
}

resource "databricks_token" "pat" {
  comment = "Terraform (created: ${time_rotating.this.rfc3339})"

  # Token is valid for 120 days but is rotated after 45 days.
  # Run `terraform apply` within 120 days to refresh before it expires.
  lifetime_seconds = 120 * 24 * 60 * 60

  depends_on = [azurerm_databricks_workspace.this]
}

resource "databricks_instance_pool" "migration_pool_integration_test" {
  instance_pool_name                    = "migration-integration-test-instance-pool"
  min_idle_instances                    = 1
  max_capacity                          = 10
  node_type_id                          = "Standard_E4d_v4"
  idle_instance_autotermination_minutes = 60
  preloaded_spark_versions              = [local.databricks_runtime_version]
  depends_on                            = [azurerm_databricks_workspace.this, databricks_token.pat]
}

# Fixes: waiting for organization to be ready
resource "time_sleep" "wait_for_instance_pool" {
  create_duration = "60s"

  depends_on = [databricks_instance_pool.migration_pool_integration_test]
}

resource "databricks_job" "migration_workflow" {
  name = "Subsystemtest"

  job_cluster {
    job_cluster_key = "subsystemtest_job_cluster"
    new_cluster {
      instance_pool_id = databricks_instance_pool.migration_pool_integration_test.id
      spark_version    = local.databricks_runtime_version
      spark_conf = {
        "fs.azure.account.oauth2.client.endpoint.${azurerm_storage_account.this.name}.dfs.core.windows.net" : "https://login.microsoftonline.com/${data.azurerm_client_config.this.tenant_id}/oauth2/token"
        "fs.azure.account.auth.type.${azurerm_storage_account.this.name}.dfs.core.windows.net" : "OAuth"
        "fs.azure.account.oauth.provider.type.${azurerm_storage_account.this.name}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
        "fs.azure.account.oauth2.client.id.${azurerm_storage_account.this.name}.dfs.core.windows.net" : databricks_secret.spn_app_id_integration_test.config_reference
        "fs.azure.account.oauth2.client.secret.${azurerm_storage_account.this.name}.dfs.core.windows.net" : databricks_secret.spn_app_secret_integration_test.config_reference
        "spark.databricks.sql.initial.catalog.name" : local.databricks_unity_catalog_name
        "spark.databricks.delta.preview.enabled" : true
        "spark.databricks.io.cache.enabled" : true
        "spark.master" : "local[*, 4]"
      }
      spark_env_vars = {
        "APPI_INSTRUMENTATION_KEY"        = azurerm_application_insights.this.instrumentation_key
        "LANDING_STORAGE_ACCOUNT"         = azurerm_storage_account.this.name
        "DATALAKE_STORAGE_ACCOUNT"        = azurerm_storage_account.this.name
        "DATALAKE_SHARED_STORAGE_ACCOUNT" = azurerm_storage_account.this.name
        "AUDIT_STORAGE_ACCOUNT"           = azurerm_storage_account.this.name
        "CATALOG_NAME"                    = local.databricks_unity_catalog_name
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
    job_cluster_key = "subsystemtest_job_cluster"
  }

  depends_on = [
    time_sleep.wait_for_instance_pool, azurerm_databricks_workspace.this, databricks_token.pat
  ]
}

resource "databricks_permissions" "jobs" {
  job_id = databricks_job.migration_workflow.id

  access_control {
    group_name       = "SEC-G-Datahub-DevelopersAzure"
    permission_level = "CAN_MANAGE_RUN"
  }

  depends_on = [azurerm_databricks_workspace.this, databricks_permission_assignment.developers]
}

resource "databricks_sql_endpoint" "sql_endpoint_integration_test" {
  name                      = "SQL Endpoint for Testing"
  cluster_size              = "Medium"
  max_num_clusters          = 2
  auto_stop_mins            = 15
  enable_serverless_compute = true
  warehouse_type            = "PRO"

  # Enable preview as the statement API is currently in public preview
  channel {
    name = "CHANNEL_NAME_PREVIEW"
  }

  depends_on = [azurerm_databricks_workspace.this, databricks_token.pat]
}

resource "databricks_permissions" "databricks_sql_endpoint" {
  sql_endpoint_id = databricks_sql_endpoint.sql_endpoint_integration_test.id

  access_control {
    group_name       = "SEC-G-Datahub-DevelopersAzure"
    permission_level = "CAN_MANAGE"
  }
  depends_on = [azurerm_databricks_workspace.this, databricks_permission_assignment.developers]
}


resource "databricks_cluster" "shared_all_purpose_integration_test" {
  cluster_name       = "Shared all-purpose"
  num_workers        = 1
  instance_pool_id   = databricks_instance_pool.migration_pool_integration_test.id
  spark_version      = local.databricks_runtime_version
  data_security_mode = "USER_ISOLATION"
  spark_conf = {
    "fs.azure.account.oauth2.client.endpoint.${azurerm_storage_account.this.name}.dfs.core.windows.net" : "https://login.microsoftonline.com/${data.azurerm_client_config.this.tenant_id}/oauth2/token"
    "fs.azure.account.auth.type.${azurerm_storage_account.this.name}.dfs.core.windows.net" : "OAuth"
    "fs.azure.account.oauth.provider.type.${azurerm_storage_account.this.name}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
    "fs.azure.account.oauth2.client.id.${azurerm_storage_account.this.name}.dfs.core.windows.net" : databricks_secret.spn_app_id_integration_test.config_reference
    "fs.azure.account.oauth2.client.secret.${azurerm_storage_account.this.name}.dfs.core.windows.net" : databricks_secret.spn_app_secret_integration_test.config_reference
    "spark.databricks.sql.initial.catalog.name" : local.databricks_unity_catalog_name
    "spark.databricks.delta.preview.enabled" : true
    "spark.databricks.io.cache.enabled" : true
    "spark.master" : "local[*, 4]"
  }
  spark_env_vars = {
    "APPI_INSTRUMENTATION_KEY"        = azurerm_application_insights.this.instrumentation_key
    "LANDING_STORAGE_ACCOUNT"         = azurerm_storage_account.this.name
    "DATALAKE_STORAGE_ACCOUNT"        = azurerm_storage_account.this.name
    "DATALAKE_SHARED_STORAGE_ACCOUNT" = azurerm_storage_account.this.name
    "AUDIT_STORAGE_ACCOUNT"           = azurerm_storage_account.this.name
    "CATALOG_NAME"                    = local.databricks_unity_catalog_name
  }

  depends_on = [azurerm_databricks_workspace.this, databricks_token.pat]
}

resource "databricks_permissions" "cluster" {
  cluster_id = databricks_cluster.shared_all_purpose_integration_test.id
  access_control {
    group_name       = "SEC-G-Datahub-DevelopersAzure"
    permission_level = "CAN_ATTACH_TO"
  }

  depends_on = [azurerm_databricks_workspace.this, databricks_permission_assignment.developers]
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
    azurerm_role_assignment.kv_self
  ]
}

resource "azurerm_key_vault_secret" "kvs_databricks_dbw_domain_test_workspace_token" {
  name         = "dbw-domain-test-workspace-token"
  value        = databricks_token.pat.token_value
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_role_assignment.kv_self
  ]
}

resource "azurerm_key_vault_secret" "kvs_databricks_dbw_domain_test_workspace_url" {
  name         = "dbw-domain-test-workspace-url"
  value        = azurerm_databricks_workspace.this.workspace_url
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_role_assignment.kv_self
  ]
}

resource "azurerm_key_vault_secret" "kvs_databricks_dbw_domain_test_workspace_id" {
  name         = "dbw-domain-test-workspace-id"
  value        = azurerm_databricks_workspace.this.id
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_role_assignment.kv_self
  ]
}

resource "azurerm_key_vault_secret" "kvs_databricks_dbw_domain_test_storage_account_name" {
  name         = "dbw-domain-test-storage-account-name"
  value        = azurerm_storage_account.this.name
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_role_assignment.kv_self
  ]
}

resource "azurerm_key_vault_secret" "kvs_databricks_dbw_subsystem_test_workspace_token" {
  name         = "dbw-subsystem-test-workspace-token"
  value        = databricks_token.pat.token_value
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_role_assignment.kv_self
  ]
}

resource "azurerm_key_vault_secret" "kvs_databricks_dbw_subsystem_test_workspace_url" {
  name         = "dbw-subsystem-test-workspace-url"
  value        = azurerm_databricks_workspace.this.workspace_url
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_role_assignment.kv_self
  ]
}

resource "azurerm_key_vault_secret" "kvs_databricks_dbw_subsystem_test_workspace_id" {
  name         = "dbw-subsystem-test-workspace-id"
  value        = azurerm_databricks_workspace.this.id
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_role_assignment.kv_self
  ]
}

resource "azurerm_key_vault_secret" "kvs_databricks_dbw_subsystem_test_storage_account_name" {
  name         = "dbw-subsystem-test-storage-account-name"
  value        = azurerm_storage_account.this.name
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_role_assignment.kv_self
  ]
}

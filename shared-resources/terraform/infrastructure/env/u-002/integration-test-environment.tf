data "azurerm_client_config" "this" {}

resource "azurerm_resource_group" "integration-test-rg" {
  name     = "rg-DataHub-IntegrationTestResources-U-002"
  location = "West Europe"
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

variable "spn_ci_object_id" {
  type        = string
  description = "(Required) The Object ID of the Service principal running integration tests in CI pipelines."
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

resource "azurerm_role_assignment" "ci-spn-contributor-resource-group" {
  scope                = azurerm_resource_group.integration-test-rg.id
  role_definition_name = "Contributor"
  principal_id         = var.spn_ci_object_id
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

resource "azurerm_storage_account" "playground" {
  name                     = "samigrationplayground"
  resource_group_name      = azurerm_resource_group.integration-test-rg.name
  location                 = azurerm_resource_group.integration-test-rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
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
  scope                = azurerm_storage_account.playground.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azuread_service_principal.spn_databricks_migration.id
}

resource "azurerm_storage_container" "playground" {
  name                  = "playground"
  storage_account_name  = azurerm_storage_account.playground.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "playground_timeseries_testdata" {
  name                  = "timeseries-testdata"
  storage_account_name  = azurerm_storage_account.playground.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "playground_meteringpoints_testdata" {
  name                  = "meteringpoints-testdata"
  storage_account_name  = azurerm_storage_account.playground.name
  container_access_type = "private"
}

resource "databricks_git_credential" "ado" {
  git_username          = var.github_username
  git_provider          = "gitHub"
  personal_access_token = var.github_personal_access_token
}

resource "azurerm_databricks_workspace" "playground" {
  name                = "databricks-playground"
  resource_group_name = azurerm_resource_group.integration-test-rg.name
  location            = azurerm_resource_group.integration-test-rg.location
  sku                 = "premium"
}

resource "azuread_application_password" "secret" {
  application_object_id = azuread_application.app_databricks_migration.object_id
}

resource "databricks_secret_scope" "spn_app_id" {
  name = "spn-id-scope"
}

resource "databricks_secret" "spn_app_id" {
  key          = "spn_app_id"
  string_value = azuread_application.app_databricks_migration.application_id
  scope        = databricks_secret_scope.spn_app_id.id
}

resource "databricks_secret_scope" "spn_app_secret" {
  name = "spn-secret-scope"
}

resource "databricks_secret" "spn_app_secret" {
  key          = "spn_app_secret"
  string_value = azuread_application_password.secret.value
  scope        = databricks_secret_scope.spn_app_secret.id
}

data "external" "databricks_token_playground" {
  program = ["pwsh", "${path.cwd}/scripts/generate-pat-token.ps1", azurerm_databricks_workspace.playground.id, "https://${azurerm_databricks_workspace.playground.workspace_url}"]
  depends_on = [
    azurerm_databricks_workspace.playground
  ]
}

data "databricks_spark_version" "latest_lts" {
  long_term_support = true
}

module "kvs_databricks_dbw_playground_workspace_token" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name         = "dbw-playground-workspace-token"
  value        = data.external.databricks_token_playground.result.pat_token
  key_vault_id = azurerm_key_vault.integration-test-kv.id
}

module "kvs_databricks_dbw_playground_workspace_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name         = "dbw-playground-workspace-url"
  value        = azurerm_databricks_workspace.playground.workspace_url
  key_vault_id = azurerm_key_vault.integration-test-kv.id
}

module "kvs_databricks_dbw_playground_workspace_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name         = "dbw-playground-workspace-id"
  value        = azurerm_databricks_workspace.playground.id
  key_vault_id = azurerm_key_vault.integration-test-kv.id
}

resource "databricks_instance_pool" "my_pool" {
  instance_pool_name                    = "migration-playground-instance-pool"
  min_idle_instances                    = 0
  max_capacity                          = 5
  node_type_id                          = "Standard_DS3_v2"
  idle_instance_autotermination_minutes = 60
}

resource "databricks_job" "migration_playground_workflow" {
  name = "Landing_To_Wholesale_Gold_Fully_In_Playground"

  job_cluster {
    job_cluster_key = "playground_job_cluster"
    new_cluster {
      instance_pool_id = databricks_instance_pool.my_pool.id
      spark_version    = data.databricks_spark_version.latest_lts.id
      spark_conf = {
        "fs.azure.account.oauth2.client.endpoint.${azurerm_storage_account.playground.name}.dfs.core.windows.net" : "https://login.microsoftonline.com/${var.tenant_id}/oauth2/token"
        "fs.azure.account.auth.type.${azurerm_storage_account.playground.name}.dfs.core.windows.net" : "OAuth"
        "fs.azure.account.oauth.provider.type.${azurerm_storage_account.playground.name}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
        "fs.azure.account.oauth2.client.id.${azurerm_storage_account.playground.name}.dfs.core.windows.net" : databricks_secret.spn_app_id.config_reference
        "fs.azure.account.oauth2.client.secret.${azurerm_storage_account.playground.name}.dfs.core.windows.net" : databricks_secret.spn_app_secret.config_reference
        "spark.databricks.delta.preview.enabled" : true
        "spark.databricks.io.cache.enabled" : true
        "spark.master" : "local[*, 4]"
      }
      spark_env_vars = {
        "APPI_INSTRUMENTATION_KEY" = azurerm_key_vault_secret.kvs-appi-instrumentation-key.value
      }
    }
  }

  git_source {
    url      = "https://github.com/Energinet-DataHub/opengeh-migration.git"
    provider = "gitHub"
    branch   = "main"
  }

  task {
    task_key = "playground_setup"

    library {
      whl = "dbfs:/opengeh-migration/GEHMigrationPackage-1.0-py3-none-any.whl"
    }

    notebook_task {
      notebook_path = "source/DataMigration/config/playground_setup"
      base_parameters = {
        batch_execution = true
        use_playground  = true
      }
    }
    job_cluster_key = "playground_job_cluster"
  }

  task {
    task_key = "check_schemas"
    depends_on {
      task_key = "playground_setup"
    }

    notebook_task {
      notebook_path = "source/DataMigration/config/schema_validation"
      base_parameters = {
        BatchExecution = true
      }
    }
    job_cluster_key = "playground_job_cluster"
  }

  task {
    task_key = "autoloader_time_series"
    depends_on {
      task_key = "check_schemas"
    }

    notebook_task {
      notebook_path = "source/DataMigration/bronze/autoloader_time_series"
      base_parameters = {
        BatchExecution = true
      }
    }
    job_cluster_key = "playground_job_cluster"
  }

  task {
    task_key = "autoloader_metering_points"
    depends_on {
      task_key = "check_schemas"
    }

    notebook_task {
      notebook_path = "source/DataMigration/bronze/autoloader_metering_points"
      base_parameters = {
        BatchExecution = true
      }
    }
    job_cluster_key = "playground_job_cluster"
  }

  task {
    task_key = "bronze_to_silver_time_series"
    depends_on {
      task_key = "autoloader_time_series"
    }

    notebook_task {
      notebook_path = "source/DataMigration/silver/time_series"
      base_parameters = {
        BatchExecution = true
      }
    }
    job_cluster_key = "playground_job_cluster"
  }

  task {
    task_key = "bronze_to_silver_metering_points"
    depends_on {
      task_key = "autoloader_metering_points"
    }

    notebook_task {
      notebook_path = "source/DataMigration/silver/metering_points"
      base_parameters = {
        BatchExecution = true
      }
    }
    job_cluster_key = "playground_job_cluster"
  }

  task {
    task_key = "silver_to_gold_time_series"
    depends_on {
      task_key = "bronze_to_silver_time_series"
    }
    depends_on {
      task_key = "bronze_to_silver_metering_points"
    }

    notebook_task {
      notebook_path = "source/DataMigration/gold/wholesale_time_series"
      base_parameters = {
        BatchExecution = true
      }
    }
    job_cluster_key = "playground_job_cluster"
  }

  task {
    task_key = "silver_to_gold_metering_points"
    depends_on {
      task_key = "bronze_to_silver_metering_points"
    }

    notebook_task {
      notebook_path = "source/DataMigration/gold/wholesale_metering_points"
      base_parameters = {
        BatchExecution = true
      }
    }
    job_cluster_key = "playground_job_cluster"
  }

  task {
    task_key = "time_series_system_test"
    depends_on {
      task_key = "silver_to_gold_time_series"
    }

    notebook_task {
      notebook_path = "source/DataMigration/systemtest/time_series/time_series_tests"
      base_parameters = {
        BatchExecution = true
      }
    }
    job_cluster_key = "playground_job_cluster"
  }

  task {
    task_key = "metering_points_system_test"
    depends_on {
      task_key = "silver_to_gold_metering_points"
    }

    notebook_task {
      notebook_path = "source/DataMigration/systemtest/metering_points/metering_points_tests"
      base_parameters = {
        BatchExecution = true
      }
    }
    job_cluster_key = "playground_job_cluster"
  }

  depends_on = [
    databricks_instance_pool.my_pool
  ]
}

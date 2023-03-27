resource "databricks_secret_scope" "spn_app_id" {
  name = "spn-id-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
}

resource "databricks_secret" "spn_app_id" {
  key          = "spn_app_id"
  string_value = azuread_application.app_databricks.application_id
  scope        = databricks_secret_scope.spn_app_id.id
}

resource "databricks_secret_scope" "spn_app_secret" {
  name = "spn-secret-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
}

resource "databricks_secret" "spn_app_secret" {
  key          = "spn_app_secret"
  string_value = azuread_application_password.secret.value
  scope        = databricks_secret_scope.spn_app_secret.id
}

resource "databricks_secret" "appi_instrumentation_key" {
  key          = "appi_instrumentation_key"
  string_value = data.azurerm_key_vault_secret.appi_instrumentation_key.value
  scope        = databricks_secret_scope.spn_app_secret.id
}

resource "databricks_sql_global_config" "this" {
  security_policy = "DATA_ACCESS_CONTROL"
  data_access_config = {
    "spark.hadoop.fs.azure.account.auth.type.${module.st_migrations.name}.dfs.core.windows.net" : "OAuth",
    "spark.hadoop.fs.azure.account.oauth.provider.type.${module.st_migrations.name}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
    "spark.hadoop.fs.azure.account.oauth2.client.id.${module.st_migrations.name}.dfs.core.windows.net" : databricks_secret.spn_app_id.config_reference,
    "spark.hadoop.fs.azure.account.oauth2.client.secret.${module.st_migrations.name}.dfs.core.windows.net" : databricks_secret.spn_app_secret.config_reference,
    "spark.hadoop.fs.azure.account.oauth2.client.endpoint.${module.st_migrations.name}.dfs.core.windows.net" : "https://login.microsoftonline.com/${var.tenant_id}/oauth2/token",

    "spark.hadoop.fs.azure.account.auth.type.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : "OAuth",
    "spark.hadoop.fs.azure.account.oauth.provider.type.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider",
    "spark.hadoop.fs.azure.account.oauth2.client.id.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : databricks_secret.spn_app_id.config_reference,
    "spark.hadoop.fs.azure.account.oauth2.client.secret.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : databricks_secret.spn_app_secret.config_reference,
    "spark.hadoop.fs.azure.account.oauth2.client.endpoint.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : "https://login.microsoftonline.com/${var.tenant_id}/oauth2/token"
  }
}

resource "databricks_job" "this" {
  name = "Landing_To_Gold"

  job_cluster {
    job_cluster_key = "metering_point_job_cluster"
    new_cluster {
      num_workers   = 0
      spark_version = data.databricks_spark_version.latest_lts.id
      node_type_id  = "Standard_DS5_v2"
      spark_conf = {
        "fs.azure.account.oauth2.client.endpoint.${data.azurerm_storage_account.drop.name}.dfs.core.windows.net" : "https://login.microsoftonline.com/${var.tenant_id}/oauth2/token"
        "fs.azure.account.oauth2.client.endpoint.${module.st_migrations.name}.dfs.core.windows.net" : "https://login.microsoftonline.com/${var.tenant_id}/oauth2/token"
        "fs.azure.account.oauth2.client.endpoint.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : "https://login.microsoftonline.com/${var.tenant_id}/oauth2/token"
        "fs.azure.account.auth.type.${data.azurerm_storage_account.drop.name}.dfs.core.windows.net" : "OAuth"
        "fs.azure.account.auth.type.${module.st_migrations.name}.dfs.core.windows.net" : "OAuth"
        "fs.azure.account.auth.type.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : "OAuth"
        "fs.azure.account.oauth.provider.type.${data.azurerm_storage_account.drop.name}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
        "fs.azure.account.oauth.provider.type.${module.st_migrations.name}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
        "fs.azure.account.oauth.provider.type.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
        "fs.azure.account.oauth2.client.id.${data.azurerm_storage_account.drop.name}.dfs.core.windows.net" : databricks_secret.spn_app_id.config_reference
        "fs.azure.account.oauth2.client.id.${module.st_migrations.name}.dfs.core.windows.net" : databricks_secret.spn_app_id.config_reference
        "fs.azure.account.oauth2.client.id.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : databricks_secret.spn_app_id.config_reference
        "fs.azure.account.oauth2.client.secret.${data.azurerm_storage_account.drop.name}.dfs.core.windows.net" : databricks_secret.spn_app_secret.config_reference
        "fs.azure.account.oauth2.client.secret.${module.st_migrations.name}.dfs.core.windows.net" : databricks_secret.spn_app_secret.config_reference
        "fs.azure.account.oauth2.client.secret.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : databricks_secret.spn_app_secret.config_reference
        "spark.databricks.delta.preview.enabled" : true
        "spark.databricks.io.cache.enabled" : true
        "spark.master" : "local[*, 4]"
      }
      spark_env_vars = {
        "APPI_INSTRUMENTATION_KEY"        = databricks_secret.appi_instrumentation_key.config_reference
        "LANDING_STORAGE_ACCOUNT"         = data.azurerm_storage_account.drop.name # Should we use this or another datalake for dump
        "DATALAKE_STORAGE_ACCOUNT"        = module.st_migrations.name
        "DATALAKE_SHARED_STORAGE_ACCOUNT" = data.azurerm_key_vault_secret.st_data_lake_name.value
      }
    }
  }

  job_cluster {
    job_cluster_key = "time_series_job_cluster"
    new_cluster {
      num_workers   = 0
      spark_version = data.databricks_spark_version.latest_lts.id
      node_type_id  = "Standard_DS5_v2"
      spark_conf = {
        "fs.azure.account.oauth2.client.endpoint.${data.azurerm_storage_account.drop.name}.dfs.core.windows.net" : "https://login.microsoftonline.com/${var.tenant_id}/oauth2/token"
        "fs.azure.account.oauth2.client.endpoint.${module.st_migrations.name}.dfs.core.windows.net" : "https://login.microsoftonline.com/${var.tenant_id}/oauth2/token"
        "fs.azure.account.oauth2.client.endpoint.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : "https://login.microsoftonline.com/${var.tenant_id}/oauth2/token"
        "fs.azure.account.auth.type.${data.azurerm_storage_account.drop.name}.dfs.core.windows.net" : "OAuth"
        "fs.azure.account.auth.type.${module.st_migrations.name}.dfs.core.windows.net" : "OAuth"
        "fs.azure.account.auth.type.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : "OAuth"
        "fs.azure.account.oauth.provider.type.${data.azurerm_storage_account.drop.name}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
        "fs.azure.account.oauth.provider.type.${module.st_migrations.name}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
        "fs.azure.account.oauth.provider.type.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
        "fs.azure.account.oauth2.client.id.${data.azurerm_storage_account.drop.name}.dfs.core.windows.net" : databricks_secret.spn_app_id.config_reference
        "fs.azure.account.oauth2.client.id.${module.st_migrations.name}.dfs.core.windows.net" : databricks_secret.spn_app_id.config_reference
        "fs.azure.account.oauth2.client.id.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : databricks_secret.spn_app_id.config_reference
        "fs.azure.account.oauth2.client.secret.${data.azurerm_storage_account.drop.name}.dfs.core.windows.net" : databricks_secret.spn_app_secret.config_reference
        "fs.azure.account.oauth2.client.secret.${module.st_migrations.name}.dfs.core.windows.net" : databricks_secret.spn_app_secret.config_reference
        "fs.azure.account.oauth2.client.secret.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : databricks_secret.spn_app_secret.config_reference
        "spark.databricks.delta.preview.enabled" : true
        "spark.databricks.io.cache.enabled" : true
        "spark.master" : "local[*, 4]"
      }
      spark_env_vars = {
        "APPI_INSTRUMENTATION_KEY"        = databricks_secret.appi_instrumentation_key.config_reference
        "LANDING_STORAGE_ACCOUNT"         = data.azurerm_storage_account.drop.name # Should we use this or another datalake for dump
        "DATALAKE_STORAGE_ACCOUNT"        = module.st_migrations.name
        "DATALAKE_SHARED_STORAGE_ACCOUNT" = data.azurerm_key_vault_secret.st_data_lake_name.value
      }
    }
  }

  git_source {
    url      = "https://github.com/Energinet-DataHub/opengeh-migration.git"
    provider = "gitHub"
    branch   = "main"
  }

  task {
    # When semantic versioning is ready, remove this uuid as this is only added to trigger a wheel on each merge into main
    task_key = local.task_workflow_setup_trigger
    library {
      whl = "dbfs:/opengeh-migration/GEHMigrationPackage-1.0-py3-none-any.whl"
    }

    notebook_task {
      notebook_path = "source/DataMigration/config/workspace_setup"
      base_parameters = {
        batch_execution                 = true
        landing_storage_account         = data.azurerm_storage_account.drop.name # Should we use this or another datalake for dump
        datalake_storage_account        = module.st_migrations.name
        datalake_shared_storage_account = data.azurerm_key_vault_secret.st_data_lake_name.value
        metering_point_container        = "dh2-metering-point-history"
        time_series_container           = "dh2-timeseries"
      }
    }
    job_cluster_key = "metering_point_job_cluster"
  }

  task {
    task_key = "check_schemas"
    depends_on {
      task_key = local.task_workflow_setup_trigger
    }

    library {
      whl = "dbfs:/opengeh-migration/GEHMigrationPackage-1.0-py3-none-any.whl"
    }

    notebook_task {
      notebook_path = "source/DataMigration/config/schema_validation"
    }
    job_cluster_key = "time_series_job_cluster"
  }

  task {
    task_key = "autoloader_time_series"
    depends_on {
      task_key = "check_schemas"
    }

    notebook_task {
      notebook_path = "source/DataMigration/bronze/autoloader_time_series"
    }
    job_cluster_key = "time_series_job_cluster"
  }

  task {
    task_key = "autoloader_metering_points"
    depends_on {
      task_key = "check_schemas"
    }

    notebook_task {
      notebook_path = "source/DataMigration/bronze/autoloader_metering_points"
    }
    job_cluster_key = "metering_point_job_cluster"
  }

  task {
    task_key = "bronze_to_silver_time_series"
    depends_on {
      task_key = "autoloader_time_series"
    }

    notebook_task {
      notebook_path = "source/DataMigration/silver/time_series"
    }
    job_cluster_key = "time_series_job_cluster"
  }

  task {
    task_key = "bronze_to_silver_metering_points"
    depends_on {
      task_key = "autoloader_metering_points"
    }

    notebook_task {
      notebook_path = "source/DataMigration/silver/metering_points"
    }
    job_cluster_key = "metering_point_job_cluster"
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
      notebook_path = "source/DataMigration/gold/time_series"
    }
    job_cluster_key = "time_series_job_cluster"
  }

  task {
    task_key = "silver_to_gold_metering_points"
    depends_on {
      task_key = "bronze_to_silver_metering_points"
    }

    notebook_task {
      notebook_path = "source/DataMigration/gold/wholesale_metering_points"
    }
    job_cluster_key = "metering_point_job_cluster"
  }
}

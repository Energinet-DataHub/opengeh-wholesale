data "azurerm_client_config" "this" {}

locals {
  sqlServerAdminName          = "perfdbadmin"
}

resource "azurerm_mssql_server" "performance_test" {
  name                          = "mssql-perf-test-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name           = azurerm_resource_group.this.name
  location                      = azurerm_resource_group.this.location
  version                       = "12.0"
  administrator_login           = local.sqlServerAdminName
  administrator_login_password  = random_password.mssql_administrator_login_password_performance_test.result

  identity {
    type  = "SystemAssigned"
  }

  azuread_administrator {
    azuread_authentication_only = false

    login_username              = data.azurerm_client_config.this.client_id
    object_id                   = data.azurerm_client_config.this.object_id
  }

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
    ]
  }
}

resource "random_password" "mssql_administrator_login_password_performance_test" {
  length = 16
  special = true
  override_special = "_%@"
}

resource "azurerm_private_endpoint" "performance_test_private_endpoint" {
   name                = "pe-mssql-perf-test-${random_string.this.result}-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
   location            = azurerm_resource_group.this.location
   resource_group_name = azurerm_resource_group.this.name
   subnet_id           = data.azurerm_key_vault_secret.snet_private_endpoints_id.value

   private_service_connection {
    name                            = "psc-01"
    private_connection_resource_id  = azurerm_mssql_server.performance_test.id
    is_manual_connection            = false
    subresource_names               = [
       "sqlServer"
    ]
  }

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
      private_dns_zone_group,
    ]
  }
}

resource "random_string" "this" {
  length  = 5
  special = false
  upper   = false
}

resource "null_resource" "create_developer_ad_group_as_db_readers" {
  triggers = {
    always_run = "${timestamp()}"
  }

  provisioner "local-exec" {
    command     = "${path.cwd}/scripts/ensure-sql-server-is-ad-directory-reader.ps1 -sqlServerName \"${azurerm_mssql_server.performance_test.name}\" -resourceGroupName \"${azurerm_resource_group.this.name}\" -adGroupDirectoryReader \"${var.ad_group_directory_reader}\""
    interpreter = ["pwsh", "-Command"]
  }
  depends_on = [azurerm_mssql_server.performance_test]
}


module "mssqldb_edi_performance_test" {
  source                        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database?ref=v10"

  name                          = "edi-perf-test"
  project_name                  = var.domain_name_short
  environment_short             = var.environment_short
  environment_instance          = var.environment_instance
  server_id                     = azurerm_mssql_server.performance_test.id
  log_analytics_workspace_id    = data.azurerm_key_vault_secret.log_shared_id.value
  sql_server_name               = azurerm_mssql_server.performance_test.name
  max_size_gb                   = 10
  sku_name                      = "GP_S_Gen5_2"
  auto_pause_delay_in_minutes   = "-1"
  min_capacity                  = 1
  developer_ad_group_name       = var.developer_ad_group_name
}

module "mssql_database_application_access_performance_test" {
  source                  = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-application-access?ref=v10"

  sql_server_name         = azurerm_mssql_server.performance_test.name
  database_name           = module.mssqldb_edi_performance_test.name
  application_hosts_names = [
    module.func_receiver.name,
  ] 
  depends_on              = [
    module.func_receiver.name,
  ]
}

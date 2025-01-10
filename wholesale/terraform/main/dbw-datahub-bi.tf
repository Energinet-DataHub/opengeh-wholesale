# SQL warehouse for SAP BI
resource "databricks_sql_endpoint" "datahub_bi" {
  count = var.datahub_bi_endpoint_enabled ? 1 : 0

  provider         = databricks.dbw
  name             = "Datahub BI SQL Endpoint"
  cluster_size     = "Small"
  max_num_clusters = 10
  auto_stop_mins   = 15
  warehouse_type   = "PRO"
}

# Note: The code below creating SPNs and ACLs for issuing a Databricks token is a candidate for refactoring
# into a Terraform module should more consumers need this functionality in the future


data "azuread_application_published_app_ids" "well_known" {}
data "azuread_service_principal" "msgraph" {
  client_id = data.azuread_application_published_app_ids.well_known.result["MicrosoftGraph"]
}

# Microsoft Entra SPN for Datahub BI
# Resource access is inspired by this blogpost: https://stackoverflow.com/questions/63686132/error-403-user-not-authorized-when-trying-to-access-azure-databricks-api-through
resource "azuread_application" "app_datahub_bi" {
  count = var.datahub_bi_endpoint_enabled ? 1 : 0

  display_name = "sp-datahub-bi-${local.resource_suffix_with_dash}"

  required_resource_access {
    resource_app_id = data.azuread_application_published_app_ids.well_known.result["AzureDataBricks"]
    resource_access {
      id   = "739272be-e143-11e8-9f32-f2801f1b9fd1" # user_impersonation
      type = "Scope"
    }
  }
  required_resource_access {
    resource_app_id = data.azuread_application_published_app_ids.well_known.result["MicrosoftGraph"]
    resource_access {
      id   = data.azuread_service_principal.msgraph.oauth2_permission_scope_ids["User.Read"]
      type = "Scope"
    }
  }

  owners = [
    data.azuread_client_config.current.object_id
  ]
}

resource "azuread_service_principal" "spn_datahub_bi" {
  count = var.datahub_bi_endpoint_enabled ? 1 : 0

  client_id                    = azuread_application.app_datahub_bi[0].client_id
  app_role_assignment_required = false
  owners = [
    data.azuread_client_config.current.object_id
  ]
}

//This data pointer could be removed if Databricks module adds 'name' to output variables
data "azurerm_databricks_workspace" "dbw" {
  count = var.datahub_bi_endpoint_enabled ? 1 : 0

  name                = module.dbw.name
  resource_group_name = azurerm_resource_group.this.name
}

resource "azurerm_role_assignment" "spn_datahub_bi_workspace_access" {
  count = var.datahub_bi_endpoint_enabled ? 1 : 0

  scope                = data.azurerm_databricks_workspace.dbw[0].id
  role_definition_name = "Reader"
  principal_id         = azuread_service_principal.spn_datahub_bi[0].object_id
}

resource "azuread_application_password" "spn_secret" {
  count = var.datahub_bi_endpoint_enabled ? 1 : 0

  display_name   = "spn-secret"
  application_id = azuread_application.app_datahub_bi[0].id
}


# Databricks service principal
resource "databricks_service_principal" "sp_datahub_bi" {
  count = var.datahub_bi_endpoint_enabled ? 1 : 0

  provider              = databricks.dbw
  application_id        = azuread_application.app_datahub_bi[0].client_id // Link the MS Entra SPN to the Databricks SPN
  display_name          = azuread_application.app_datahub_bi[0].display_name
  databricks_sql_access = true
}

resource "databricks_permissions" "datahub_bi_sql_endpoint" {
  count = var.datahub_bi_endpoint_enabled ? 1 : 0

  provider        = databricks.dbw
  sql_endpoint_id = databricks_sql_endpoint.datahub_bi[0].id

  access_control {
    group_name       = var.databricks_readers_group.name //Allow devs with reader permissions to read/monitor access in UI
    permission_level = "CAN_MONITOR"
  }

  access_control {
    service_principal_name = azuread_application.app_datahub_bi[0].client_id //Allow the SPN to use the SQL endpoint
    permission_level       = "CAN_USE"
  }

  depends_on = [module.dbw]
}

resource "databricks_grant" "databricks_spn_database_grant_select_wholesale_results" {
  count    = var.datahub_bi_endpoint_enabled ? 1 : 0
  provider = databricks.dbw

  schema     = databricks_schema.results.id //wholesale_results
  principal  = azuread_application.app_datahub_bi[0].client_id
  privileges = ["USE_SCHEMA", "SELECT"]
}

resource "databricks_grant" "databricks_spn_database_grant_select_shared_wholesale_input" {
  count    = var.datahub_bi_endpoint_enabled ? 1 : 0
  provider = databricks.dbw

  schema     = "${data.azurerm_key_vault_secret.shared_unity_catalog_name.value}.shared_wholesale_input"
  principal  = azuread_application.app_datahub_bi[0].client_id
  privileges = ["USE_SCHEMA", "SELECT"]
}

resource "databricks_grant" "databricks_spn_database_grant_select_wholesale_sap" {
  count    = var.datahub_bi_endpoint_enabled ? 1 : 0
  provider = databricks.dbw

  schema     = databricks_schema.sap.id //wholesale_sap
  principal  = azuread_application.app_datahub_bi[0].client_id
  privileges = ["USE_SCHEMA", "SELECT"]
}

//Needed to interact with objects in schemas
//See https://docs.databricks.com/en/data-governance/unity-catalog/manage-privileges/privileges.html#use-catalog for details
resource "databricks_grant" "databricks_spn_database_grant_use_catalog" {
  count    = var.datahub_bi_endpoint_enabled ? 1 : 0
  provider = databricks.dbw

  catalog    = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  principal  = azuread_application.app_datahub_bi[0].client_id
  privileges = ["USE_CATALOG"]
}

//Create a group for external token users and grant CAN_USE permission to the group
//As there can only be one 'authorization = "tokens"' permissions resource per workspace, it should be attached to a group
//as it will be much easier to add more SPNs for external access later on rather than applying the role directly to the SPN
//See https://registry.terraform.io/providers/databrickslabs/databricks/latest/docs/resources/permissions#token-usage for details
resource "databricks_group" "external_token_users" {
  count = var.datahub_bi_endpoint_enabled ? 1 : 0

  provider     = databricks.dbw
  display_name = "ExternalTokenUsers"
}

resource "databricks_group_member" "sp_datahub_bi_exttokenusers_membership" {
  count = var.datahub_bi_endpoint_enabled ? 1 : 0

  provider  = databricks.dbw
  group_id  = databricks_group.external_token_users[0].id
  member_id = databricks_service_principal.sp_datahub_bi[0].id
}

resource "databricks_permissions" "token_access" {
  count = var.datahub_bi_endpoint_enabled ? 1 : 0

  provider = databricks.dbw
  # Note: There can only be one 'authorization = "tokens"' permissions resource per workspace
  # See https://registry.terraform.io/providers/databrickslabs/databricks/latest/docs/resources/permissions#token-usage for details
  authorization = "tokens"

  access_control {
    group_name       = databricks_group.external_token_users[0].display_name
    permission_level = "CAN_USE"
  }
}


# Databricks token for Datahub BI Service principal
resource "time_rotating" "datahub_bi_token" {
  count = var.datahub_bi_endpoint_enabled ? 1 : 0

  rotation_days = 365 #  --> Rotate once per year
}

# Token is valid for 400 days but is rotated after 365 days.
# This allows Data Delivery team to update to a new token before the old one expires (after 365 days)
# Outlaws have a schedule to reach out to Data Delivery team and notify them that they should update to a new token
# It's not pretty at all, but it's the best we can do with the current setup
# Bonusinfo: I kind of like the fact that CoPilot finished the previous sentence for me after typing 'It's not pretty'....
resource "shell_script" "create_datahub_bi_token" {
  count = var.datahub_bi_endpoint_enabled ? 1 : 0

  lifecycle_commands {
    create = file("${path.root}/scripts/create-databricks-token.sh")
    delete = "echo 'New token to be created'"
  }

  environment = {
    workspaceUrl       = "${module.dbw.workspace_url}"
    lifetimeSeconds    = "${60 * 60 * 24 * 400}" # Token is valid for 400 days
    tenantId           = "${data.azuread_client_config.current.tenant_id}"
    clientId           = "${azuread_application.app_datahub_bi[0].client_id}"
    clientSecret       = "${resource.azuread_application_password.spn_secret[0].value}"
    keyvaultName       = module.kv_internal.name
    keyvaultSecretName = "${azuread_application.app_datahub_bi[0].display_name}-dbw-token"
  }

  interpreter = ["/bin/bash", "-c"]

  triggers = {
    rotation     = time_rotating.datahub_bi_token[0].rfc3339
    workspace    = module.dbw.workspace_url,
    clientId     = azuread_application.app_datahub_bi[0].client_id,
    clientSecret = resource.azuread_application_password.spn_secret[0].value
  }

  // One has to manually maintain dependencies as Terraform has no idea what the script does.
  // Which is why scripts in a DSL like Terraform is very much an antipattern and should be avoided if possible
  depends_on = [
    time_rotating.datahub_bi_token,
    module.dbw,
    module.kv_internal,
    databricks_permissions.datahub_bi_sql_endpoint,
    databricks_group_member.sp_datahub_bi_exttokenusers_membership,
    azurerm_role_assignment.spn_datahub_bi_workspace_access,
    databricks_service_principal.sp_datahub_bi,
    databricks_group.external_token_users,
    databricks_permissions.token_access
  ]
}


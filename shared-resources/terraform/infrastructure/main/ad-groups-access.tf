# Give all developers access Controlplane Reader access to all environments

# Also ensure that Terraform state dataplane access is denied on all environments, even if
# overrides in specific environments give storage account dataplane access to developers on subscription level

# Furthermore, allow developers to read config settings in Function apps and App Services

data "azurerm_resource_group" "rg_tfstate" {
  count = 1
  name = "rg-tfs-${var.environment_short}-${var.region_short}-${var.environment_instance}"
}

resource "azurerm_role_assignment" "developers_subscription_reader" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Reader"
  principal_id         = var.developers_security_group_object_id
}

resource "azurerm_role_definition" "deny_dataplane_access_to_tfs_rg" {
  count       = 1
  name        = "datahub-deny-dataplane-access-to-tfs-rg-${var.environment_short}-${var.region_short}-${var.environment_instance}"
  scope       = data.azurerm_resource_group.rg_tfstate[0].id
  description = "Denies dataplane access to Terraform state"

  permissions {
    not_data_actions = ["Microsoft.Storage/*"]
  }
}

//Deny developers dataplane access to Terraform state on all environments
resource "azurerm_role_assignment" "deny_developer_dataplane_access_to_tfs_rg" {
  count                = 1
  scope                = data.azurerm_resource_group.rg_tfstate[0].id
  role_definition_name = resource.azurerm_role_definition.deny_dataplane_access_to_tfs_rg[0].name
  principal_id         = var.developers_security_group_object_id
}


// Allow platform team read access to shared keyvault on all environments
resource "azurerm_key_vault_access_policy" "platformteam_shared_keyvault_read_access" {
  count = 1
  key_vault_id = module.kv_shared.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = var.platform_team_security_group_object_id

  key_permissions = [
    "Get",
    "List"
  ]

  secret_permissions = [
    "Get",
    "List"
  ]

  certificate_permissions = [
    "Get",
    "List"
  ]
}


// There is no built-in role for reading appsettings :o/
// Reference: https://github.com/MicrosoftDocs/azure-docs/issues/59847#issuecomment-871298764
resource "azurerm_role_definition" "app_config_settings_read_access" {
count         = 1
  name        = "datahub-app-config-settings-read-access-${var.environment_short}-${var.region_short}-${var.environment_instance}"
  scope       = data.azurerm_subscription.this.id
  description = "Allow reading config settings in Function apps and App Services"

  permissions {
    actions = [
        "Microsoft.Web/sites/config/list/Action",
        "Microsoft.Web/sites/config/Read"
    ]
  }
}

# Allow platformteam to read config settings in Function apps and App Services on all environments
resource "azurerm_role_assignment" "platformteam_config_settings_read_access" {
  count                = 1
  scope                = data.azurerm_subscription.this.id
  role_definition_name = resource.azurerm_role_definition.app_config_settings_read_access[0].name
  principal_id         = var.platform_team_security_group_object_id
}


# There is no built-in role for managing APIM groups
resource "azurerm_role_definition" "apim_groups_contributor_access" {
  name        = "datahub-apim-groups-contributor-access-${var.environment_short}-${var.region_short}-${var.environment_instance}"
  scope       = data.azurerm_subscription.this.id
  description = "Allow adding and removing APIM users to APIM groups"

  permissions {
    actions = [
        "Microsoft.ApiManagement/service/groups/*"
    ]
  }
}

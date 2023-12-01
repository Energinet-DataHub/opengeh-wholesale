# Give all developers access Controlplane Reader access to all environments

# Also ensure that Terraform state dataplane access is denied on all environments, even if
# overrides in specific environments give storage account dataplane access to developers on subscription level

data "azurerm_subscription" "current" {
}

# data "azurerm_resource_group" "rg_tfstate" {
#   name = "rg-tfs-${var.environment_short}-${var.region_short}-${var.environment_instance}"
# }

resource "azurerm_role_assignment" "developers_subscription_reader" {
  scope                = data.azurerm_subscription.current.id
  role_definition_name = "Reader"
  principal_id         = var.developers_security_group_object_id
}

# resource "azurerm_role_definition" "deny_dataplane_access_to_tfs_rg" {
#   name        = "deny-dataplace-access-to-tfs-rg"
#   scope       = data.azurerm_resource_group.rg_tfstate.id
#   description = "Deny dataplane access to Terraform state"

#   permissions {
#     not_data_actions = ["Microsoft.Storage/*"]
#   }
# }

# resource "azurerm_role_assignment" "deny_developer_dataplane_access_to_tfs_rg" {
#   scope                = data.azurerm_resource_group.rg_tfstate.id
#   role_definition_name = resource.azurerm_role_definition.deny_dataplane_access_to_tfs_rg.name
#   principal_id         = var.developers_security_group_object_id
# }


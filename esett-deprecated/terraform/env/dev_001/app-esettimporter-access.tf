resource "azurerm_role_assignment" "importer_developer_access" {
  for_each = toset(var.developer_object_ids)

  scope                = module.app_importer.id
  role_definition_name = "Contributor"
  principal_id         = each.value
}

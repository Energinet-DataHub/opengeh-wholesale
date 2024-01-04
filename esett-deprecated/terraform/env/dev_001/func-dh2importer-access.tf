resource "azurerm_role_assignment" "dh2importer_developer_access" {
  for_each = toset(var.developer_object_ids)

  scope                = module.func_dh2importer.id
  role_definition_name = "Contributor"
  principal_id         = each.value
}

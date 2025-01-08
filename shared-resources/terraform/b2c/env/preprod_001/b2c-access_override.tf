resource "azuread_directory_role_assignment" "dbj" {
  role_id = azuread_directory_role.global_admin.template_id
}

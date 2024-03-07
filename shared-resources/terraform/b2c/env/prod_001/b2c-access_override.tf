resource "azuread_directory_role_assignment" "dbj" {
  role_id             = azuread_directory_role.global_admin.template_id
  principal_object_id = azuread_invitation.dbj[0].user_id
}

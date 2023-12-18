resource "azuread_directory_role_assignment" "xkber" {
  count = 1
  role_id             = azuread_directory_role.global_admin.template_id
  principal_object_id = azuread_invitation.xkber[0].user_id
}

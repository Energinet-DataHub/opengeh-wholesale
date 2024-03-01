#XRLFR
resource "azuread_directory_role_assignment" "xrlfr" {
  role_id             = azuread_directory_role.global_admin.template_id
  principal_object_id = azuread_invitation.xrlfr[0].user_id
}

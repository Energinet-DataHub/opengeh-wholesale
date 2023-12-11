
#DBJ
resource "azuread_directory_role_assignment" "dbj" {
  count               = 1
  role_id             = azuread_directory_role.global_admin.template_id
  principal_object_id = azuread_invitation.dbj[0].user_id
}

resource "azuread_invitation" "dbj" {
  count              = 1
  user_email_address = "dbj@energinet.dk"
  user_display_name  = "SEC-A-Greenforce-PlatformTeamAzure member"
  redirect_url       = "https://portal.azure.com"
}

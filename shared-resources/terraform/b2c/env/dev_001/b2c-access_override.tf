#NHQ
resource "azuread_directory_role_assignment" "nhq" {
  count = 1
  role_id             = azuread_directory_role.global_admin.template_id
  principal_object_id = azuread_invitation.nhq[0].user_id
}

resource "azuread_invitation" "nhq" {
  count = 1
  user_email_address = "nhq@energinet.dk"
  user_display_name = "SEC-A-Greenforce-PlatformTeamAzure member"
  redirect_url       = "https://portal.azure.com"
}

# Morten Vestergaard Hansen - MVT - Remove when he is done
resource "azuread_directory_role_assignment" "mvt" {
  count = 1

  role_id             = azuread_directory_role.global_admin.template_id
  principal_object_id = azuread_invitation.mvt[0].user_id
}

resource "azuread_invitation" "mvt" {
  count = 1

  user_email_address = "mvt@energinet.dk"
  user_display_name  = "SEC-A-Greenforce-DevelopmentTeamAzure member"
  redirect_url       = "https://portal.azure.com"
}

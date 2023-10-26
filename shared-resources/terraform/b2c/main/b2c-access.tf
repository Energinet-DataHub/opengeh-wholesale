locals {
  platform_team_members = split(",", var.platform_team_members)
}

resource "azuread_directory_role" "global_reader" {
  display_name = "Global Reader"
}

resource "azuread_directory_role_assignment" "platformteam_member" {
  count = length(local.platform_team_members)

  role_id             = azuread_directory_role.global_reader.template_id
  principal_object_id = azuread_invitation.platformteam_member[count.index].user_id
}

resource "azuread_invitation" "platformteam_member" {
  count = length(local.platform_team_members)

  user_email_address = local.platform_team_members[count.index]
  user_display_name = "SEC-A-Greenforce-PlatformTeamAzure member"
  redirect_url       = "https://portal.azure.com"
}
